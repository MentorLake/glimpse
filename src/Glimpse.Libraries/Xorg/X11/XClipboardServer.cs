using System.Reactive.Threading.Tasks;

namespace Glimpse.Libraries.Xorg.X11;

internal class XClipboardServer(X11EventQueue queue, ILogger<XClipboardServer> logger) : IDisposable
{
	private static readonly TimeSpan s_transferTimeout = TimeSpan.FromMilliseconds(750);
	private const byte XFixesSelectionNotifySubtype = 0;

	private int _xfixesEventBase = -1;

	private readonly Subject<string> _clipboardTextChanged = new();
	private readonly EventLoopScheduler _scheduler = new(threadStart => new(threadStart) { IsBackground = true, Name = "XClipboardServer" });
	private readonly XClipboardErrorHandler _errorHandler = new(logger);

	private string _servedText = "";
	private ulong _ownershipStartTime;
	private ulong _lastEventTime;
	private ulong _display;
	private ulong _rootWindow;
	private ulong _clipboardWindow;
	private ulong _clipboardProperty;
	private IDisposable _subscription;
	private XAtomsInstance _atoms;

	public IObservable<string> ClipboardTextChanged => _clipboardTextChanged;

	public void Initialize(ulong display)
	{
		_display = display;
		_rootWindow = XLib.XDefaultRootWindow(_display);
		_clipboardWindow = XLib.XCreateSimpleWindow(_display, _rootWindow, 0, 0, 1, 1, 0, 0, 0);
		_clipboardProperty = XLib.XInternAtom(_display, "GLIMPSE_CLIPBOARD", false);
		_atoms = new XAtomsInstance(_display);

		_errorHandler.Install();

		_subscription = queue.Events.Subscribe(HandleEvent);

		RegisterAsClipboardManager();
		SubscribeToOwnershipChanges();
	}

	public void Dispose()
	{
		_subscription.Dispose();
		_clipboardTextChanged.OnCompleted();
		_scheduler.Dispose();
		XLib.XSync(_display, false);
		XLib.XDestroyWindow(_display, _clipboardWindow);
		_errorHandler.Uninstall();
	}

	public void PasteIntoFocusedWindow(string text)
	{
		ClaimOwnership(text);
		FakeShiftInsert();
	}

	private void HandleEvent((XAnyEvent someEvent, IntPtr eventPointer) e)
	{
		switch ((Event)e.someEvent.type)
		{
			case Event.SelectionRequest:
				HandleSelectionRequest(Marshal.PtrToStructure<XSelectionRequestEvent>(e.eventPointer));
				break;
			case Event.SelectionClear:
				HandleSelectionClear();
				break;
			default:
				HandleXFixesEvent(e.someEvent.type, e.eventPointer);
				break;
		}
	}

	private void HandleXFixesEvent(int eventType, IntPtr eventPointer)
	{
		if (eventType != _xfixesEventBase) return;
		if (Marshal.ReadByte(eventPointer, 1) != XFixesSelectionNotifySubtype) return;
		var notification = Marshal.PtrToStructure<XFixesSelectionNotifyEvent>(eventPointer);
		HandleOwnershipChanged(notification);
	}

	private void HandleSelectionClear()
	{
		CaptureFromCurrentOwner();
	}

	private void SubscribeToOwnershipChanges()
	{
		if (!XLib.XQueryExtension(_display, "XFIXES", out _, out var firstEvent, out _))
		{
			logger.LogWarning("XFixes unavailable; clipboard copies will not be observed.");
			return;
		}

		_xfixesEventBase = firstEvent;

		XFixesLib.XFixesSelectSelectionInput(_display, _clipboardWindow, _atoms.Clipboard, XFixesLib.SetSelectionOwnerNotifyMask);
		XLib.XFlush(_display);
	}

	private void HandleOwnershipChanged(XFixesSelectionNotifyEvent notification)
	{
		if (notification.selection != _atoms.Clipboard) return;
		if (notification.owner == 0 || notification.owner == _clipboardWindow) return;
		CaptureFromCurrentOwner();
	}

	private void CaptureFromCurrentOwner()
	{
		_scheduler.ScheduleAsync(async (_, ct) =>
		{
			var observed = await ReadClipboardAsync(ct);
			if (observed.IsEmpty || observed.Text == _servedText) return;

			_servedText = observed.Text;
			_clipboardTextChanged.OnNext(observed.Text);
		});
	}

	private void HandleSelectionRequest(XSelectionRequestEvent request)
	{
		if (request.time != 0)
		{
			_lastEventTime = request.time;
		}

		if (request.selection != _atoms.Clipboard)
		{
			RejectSelectionRequest(request);
			return;
		}

		if (!IsWithinOwnershipPeriod(request.time))
		{
			RejectSelectionRequest(request);
			return;
		}

		var property = request.property != 0 ? request.property : request.target;

		if (request.target == _atoms.Multiple) ServeMultiple(request);
		else if (request.target == _atoms.Targets) ServeTargets(request, property);
		else if (request.target == _atoms.Utf8String) ServeText(request, property, Encoding.UTF8);
		else if (request.target == _atoms.String) ServeText(request, property, Encoding.Latin1);
		else if (request.target == _atoms.Text) ServeText(request, property, Encoding.UTF8);
		else if (request.target == _atoms.Timestamp) ServeTimestamp(request, property);
		else if (request.target == _atoms.SaveTargets) ServeSaveTargets(request);
		else
		{
			RejectSelectionRequest(request);
		}
	}

	private void ServeTargets(XSelectionRequestEvent request, ulong property)
	{
		var targets = new[] { _atoms.Targets, _atoms.Multiple, _atoms.SaveTargets, _atoms.Incr, _atoms.Timestamp, _atoms.Utf8String, _atoms.String, _atoms.Text };
		var targetBytes = targets.SelectMany(t => BitConverter.GetBytes((long)t)).ToArray();
		WriteSelectionPropertyAndSend(request, property, _atoms.Atom, 32, targetBytes, targets.Length);
	}

	private void ServeMultiple(XSelectionRequestEvent request)
	{
		var pairs = ReadAtomPairs(request);

		foreach (var (target, property) in pairs)
		{
			var resolvedProperty = property != 0 ? property : request.property;

			if (resolvedProperty == 0)
			{
				continue;
			}

			if (target == _atoms.Targets) ServeTargets(request, resolvedProperty);
			else if (target == _atoms.Utf8String) ServeText(request, resolvedProperty, Encoding.UTF8);
			else if (target == _atoms.String) ServeText(request, resolvedProperty, Encoding.Latin1);
			else if (target == _atoms.Text) ServeText(request, resolvedProperty, Encoding.UTF8);
			else if (target == _atoms.Timestamp) ServeTimestamp(request, resolvedProperty);
		}

		SendSelectionNotify(request, 0);
	}

	private List<(ulong Target, ulong Property)> ReadAtomPairs(XSelectionRequestEvent request)
	{
		var pairs = new List<(ulong Target, ulong Property)>();

		if (request.property == 0)
		{
			return pairs;
		}

		var result = XLib.XGetWindowProperty(_display, request.requestor, request.property, 0, 1024, true, _atoms.Atom, out _, out _, out var itemCount, out _, out var dataPointer);

		if (result != 0 || dataPointer == IntPtr.Zero)
		{
			return pairs;
		}

		var count = (int)itemCount;

		for (var i = 0; i + 1 < count; i += 2)
		{
			var target = (ulong)Marshal.ReadIntPtr(dataPointer, i * IntPtr.Size);
			var property = (ulong)Marshal.ReadIntPtr(dataPointer, (i + 1) * IntPtr.Size);
			pairs.Add((target, property));
		}

		XLib.XFree(dataPointer);
		return pairs;
	}

	private void ServeText(XSelectionRequestEvent request, ulong property, Encoding encoding)
	{
		var textBytes = encoding.GetBytes(_servedText);
		WriteSelectionPropertyAndSend(request, property, request.target, 8, textBytes, textBytes.Length);
	}

	private void ServeTimestamp(XSelectionRequestEvent request, ulong property)
	{
		var timestamp = _ownershipStartTime != 0 ? _ownershipStartTime : request.time;
		WriteSelectionPropertyAndSend(request, property, _atoms.Integer, 32, BitConverter.GetBytes(timestamp), 1);
	}

	private void ServeSaveTargets(XSelectionRequestEvent request)
	{
		var property = request.property != 0 ? request.property : request.target;
		SendSelectionNotify(request, property);
		RetainRequestorContents(request);
	}

	private void RetainRequestorContents(XSelectionRequestEvent request)
	{
		_scheduler.ScheduleAsync(async (_, ct) =>
		{
			var retained = await ReadFromOwnerAsync(request.owner, ct);
			if (retained.IsEmpty || retained.Text == _servedText) return;

			ClaimOwnership(retained.Text);
			_clipboardTextChanged.OnNext(retained.Text);
		});
	}

	private void RegisterAsClipboardManager()
	{
		XLib.XSetSelectionOwner(_display, _atoms.ClipboardManager, _clipboardWindow, XLib.CurrentTime);
		XLib.XFlush(_display);

		if (XLib.XGetSelectionOwner(_display, _atoms.ClipboardManager) != _clipboardWindow)
		{
			logger.LogWarning("Another clipboard manager already owns CLIPBOARD_MANAGER.");
		}
	}

	private void ClaimOwnership(string text)
	{
		_servedText = text;
		_ownershipStartTime = CurrentServerTime();
		XLib.XSetSelectionOwner(_display, _atoms.Clipboard, _clipboardWindow, _ownershipStartTime);
		XLib.XFlush(_display);
	}

	private async Task<ClipboardSnapshot> ReadClipboardAsync(CancellationToken cancellationToken)
	{
		var owner = XLib.XGetSelectionOwner(_display, _atoms.Clipboard);
		if (owner == 0 || owner == _clipboardWindow) return ClipboardSnapshot.Empty;
		return await ReadFromOwnerAsync(owner, cancellationToken);
	}

	private async Task<ClipboardSnapshot> ReadFromOwnerAsync(ulong owner, CancellationToken cancellationToken)
	{
		try
		{
			XLib.XDeleteProperty(_display, _clipboardWindow, _clipboardProperty);

			var textArrived = queue.Events
				.Where(e => e.someEvent.type == (int)Event.SelectionNotify)
				.Select(e => Marshal.PtrToStructure<XSelectionEvent>(e.eventPointer))
				.Where(e => e.requestor == _clipboardWindow && e.property == _clipboardProperty)
				.Select(e => e.ToRequestorWindowRef().GetStringProperty(e.property, delete: true) ?? "")
				.Take(1)
				.Timeout(s_transferTimeout)
				.ToTask(cancellationToken);

			XLib.XConvertSelection(_display, _atoms.Clipboard, _atoms.Utf8String, _clipboardProperty, _clipboardWindow, XLib.CurrentTime);
			XLib.XFlush(_display);

			var text = await textArrived;
			return new ClipboardSnapshot(text, CurrentServerTime());
		}
		catch (TimeoutException)
		{
			logger.LogDebug("Timed out reading clipboard contents from {Owner}.", owner);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception e)
		{
			logger.LogError(e, "Clipboard capture error.");
		}

		return ClipboardSnapshot.Empty;
	}

	private bool IsWithinOwnershipPeriod(ulong requestTime)
	{
		if (requestTime == 0) return true;
		return requestTime >= _ownershipStartTime;
	}

	private ulong CurrentServerTime()
	{
		return _lastEventTime;
	}

	private void FakeShiftInsert()
	{
		var insertKeycode = (uint)XLib.XKeysymToKeycode(_display, (ulong)GdkConstants.KEY_Insert);
		var shiftKeycode = (uint)XLib.XKeysymToKeycode(_display, (ulong)GdkConstants.KEY_Shift_L);

		XTestLib.XTestFakeKeyEvent(_display, shiftKeycode, true, 0);
		XTestLib.XTestFakeKeyEvent(_display, insertKeycode, true, 0);
		Thread.Sleep(50);
		XTestLib.XTestFakeKeyEvent(_display, insertKeycode, false, 0);
		XTestLib.XTestFakeKeyEvent(_display, shiftKeycode, false, 0);
		XLib.XFlush(_display);
	}

	private void WriteSelectionPropertyAndSend(XSelectionRequestEvent request, ulong property, ulong type, int format, byte[] data, int length)
	{
		XLib.XChangeProperty(_display, request.requestor, property, type, format, (int)GdkPropMode.GDK_PROP_MODE_REPLACE, data, length);
		SendSelectionNotify(request, property);
	}

	private void RejectSelectionRequest(XSelectionRequestEvent request) => SendSelectionNotify(request, property: 0);

	private void SendSelectionNotify(XSelectionRequestEvent request, ulong property)
	{
		var selectionEvent = new XSelectionEvent
		{
			type = (int)Event.SelectionNotify,
			display = _display,
			requestor = request.requestor,
			selection = request.selection,
			target = request.target,
			property = property,
			time = request.time,
			send_event = false
		};

		request.ToRequestorWindowRef().SendEvent(selectionEvent);
	}
}
