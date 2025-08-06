using System.Buffers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;
using MentorLake.GObject;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Libraries.Xorg.X11;

internal class XLibAdaptorService(IHostApplicationLifetime applicationLifetime) : IDisposable
{
	private XWindowRef _rootWindowRef;
	private readonly Subject<IObservable<WindowProperties>> _windows = new();
	private readonly Subject<(XAnyEvent someEvent, IntPtr eventPointer)> _events = new();
	private readonly Subject<XWindowRef> _focusChanged = new();

	public IObservable<XWindowRef> FocusChanged => _focusChanged;
	public IObservable<IObservable<WindowProperties>> Windows => _windows;

	public Task InitializeAsync()
	{
		XLib.XInitThreads();

		var display = XLib.XOpenDisplay(0);
		var rootWindow = XLib.XDefaultRootWindow(display);

		_rootWindowRef = new XWindowRef() { Window = rootWindow, Display = display };

		var windowEventMask = EventMask.SubstructureNotifyMask | EventMask.PropertyChangeMask | EventMask.KeyPressMask | EventMask.KeyReleaseMask;
		var windowEvents = _events.Where(e => e.someEvent.window == rootWindow).Publish();

		XLib.XSelectInput(display, rootWindow, windowEventMask);

		windowEvents.ObservePropertyEvent()
			.ObserveULongArrayProperty(XAtoms.NetActiveWindow)
			.Subscribe(array => _focusChanged.OnNext(_rootWindowRef with { Window = array[0] }));

		Observable.Return(_rootWindowRef.GetULongArray(XAtoms.NetClientList))
			.Concat(windowEvents
				.ObservePropertyEvent()
				.ObserveULongArrayProperty(XAtoms.NetClientList))
			.Select(windows => windows.Select(w => _rootWindowRef with { Window = w }).Where(w => w.IsNormalWindow()).ToArray())
			.UnbundleMany(w => w)
			.Subscribe(windowObservable =>
			{
				var windowRef = windowObservable.Key;
				var pid = windowRef.GetPid();
				XLib.XSelectInput(windowRef.Display, windowRef.Window, windowEventMask);

				windowObservable
					.TakeLast(1)
					.Subscribe(_ => XLib.XSelectInput(windowRef.Display, windowRef.Window, EventMask.NoEventMask));

				var propertyChangeObs = _events.Where(e => e.someEvent.window == windowRef.Window)
					.TakeUntil(windowObservable.TakeLast(1))
					.ObservePropertyEvent()
					.Publish();

				var titleObs = Observable.Return(windowRef.GetStringProperty(XAtoms.NetWmName)).Concat(propertyChangeObs.ObserveStringProperty(XAtoms.NetWmName));
				var iconObs = Observable.Return(windowRef.GetIcons()).Concat(propertyChangeObs.ObserveIcons(XAtoms.NetWmIcon));
				var iconNameObs = Observable.Return(windowRef.GetStringProperty(XAtoms.NetWmIconName)).Concat(propertyChangeObs.ObserveStringProperty(XAtoms.NetWmIconName));

				var defaultScreenshotObs = iconObs
					.Select(icons => icons?.MaxBy(i => i.GetWidth()))
					.Select(x => x?.ScaleToFit(300, 300));

				var stateObs = Observable.Return(windowRef.GetAtomArray(XAtoms.NetWmState))
					.Concat(propertyChangeObs.ObserveAtomArray(XAtoms.NetWmState))
					.Select(s => s.Where(x => x == XAtoms.NetWmStateDemandsAttention).ToList())
					.DistinctUntilChanged((s1, s2) => s1.SequenceEqual(s2));

				var allowedActionsObs = Observable.Return(windowRef.GetAtomNameArray(XAtoms.NetWmAllowedActions).ToList())
					.Concat(propertyChangeObs.ObserveAtomNameArray(XAtoms.NetWmAllowedActions))
					.Select(s => ParseWindowActions(s).Where(l => l == AllowedWindowActions.Close).ToArray())
					.DistinctUntilChanged((s1, s2) => s1.SequenceEqual(s2));

				XLib.XGetClassHint(windowRef.Display, windowRef.Window, out var classHint);

				var windowPropsObs = titleObs
					.CombineLatest(iconObs, iconNameObs, stateObs, allowedActionsObs, defaultScreenshotObs)
					.Select(t => new WindowProperties()
					{
						WindowRef = windowRef,
						ClassHintName = classHint.res_name,
						ClassHintClass = classHint.res_class,
						IconName = t.Third,
						Icons = t.Second,
						Title = t.First,
						AllowActions = t.Fifth,
						Pid = pid,
						DemandsAttention = t.Fourth.Contains(XAtoms.NetWmStateDemandsAttention),
						DefaultScreenshot = t.Sixth
					})
					.Throttle(TimeSpan.FromMilliseconds(250))
					.DistinctUntilChanged()
					.TakeUntil(windowObservable.TakeLast(1))
					.Replay(1);

				_windows.OnNext(windowPropsObs);
				windowPropsObs.Connect();
				propertyChangeObs.Connect();
			});

		windowEvents.Connect();

		Task.Run(() => WatchEventsAsync(applicationLifetime.ApplicationStopping));

		return Task.CompletedTask;
	}

	private AllowedWindowActions[] ParseWindowActions(List<string> x11WindowActions)
	{
		var results = new LinkedList<AllowedWindowActions>();

		foreach (var a in x11WindowActions)
		{
			var words = a.ToLower().Split("_", StringSplitOptions.RemoveEmptyEntries);
			var wordsProperCased = words.Select(s => char.ToUpper(s[0]) + s[1..]);
			var normalizedName = string.Join("", wordsProperCased.Skip(3));

			if (normalizedName.Contains("Maximize"))
			{
				results.AddLast(AllowedWindowActions.Maximize);
			}
			else if (Enum.TryParse(normalizedName, out AllowedWindowActions enumValue))
			{
				results.AddLast(enumValue);
			}
		}

		results.AddLast(AllowedWindowActions.Maximize);
		results.AddLast(AllowedWindowActions.Minimize);
		return results.Distinct().ToArray();
	}

	private async Task WatchEventsAsync(CancellationToken cancellationToken)
	{
		var eventPointer = Marshal.AllocHGlobal(24 * sizeof(long));
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(25));

		while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
		{
			while (XLib.XCheckMaskEvent(_rootWindowRef.Display, (long) EventMask.PropertyChangeMask, eventPointer))
			{
				var someEvent = Marshal.PtrToStructure<XAnyEvent>(eventPointer);
				_events.OnNext((someEvent, eventPointer));
			}
		}

		XLib.XFree(eventPointer);
	}

	public void Dispose()
	{
		XLib.XCloseDisplay(_rootWindowRef.Display);
	}

	private LinkedList<ulong> GetParents(XWindowRef windowRef)
	{
		var results = new LinkedList<ulong>();
		var current = windowRef.Window;

		while (current != 0)
		{
			results.AddLast(current);
			XLib.XQueryTree(windowRef.Display, current, out _, out var currentParent, out _, out _);
			current = currentParent;
		}

		return results;
	}

	public void ToggleWindowVisibility(XWindowRef windowRef)
	{
		XLib.XGetInputFocus(windowRef.Display, out var focusedWindow, out _);
		var focusedWindowRef = windowRef with { Window = focusedWindow };
		var windowHasFocus = GetParents(focusedWindowRef).Any(w => w == windowRef.Window);
		var iconified = windowRef.GetULongArray(XAtoms.NetWmState).Any(a => a == XAtoms.NetWmStateHidden);

		if (iconified || !windowHasFocus)
		{
			XLib.XMapRaised(windowRef.Display, windowRef.Window);
			XLib.XSetInputFocus(windowRef.Display, windowRef.Window, 0, 0);
		}
		else
		{
			XLib.XIconifyWindow(windowRef.Display, windowRef.Window, 0);
		}

		XLib.XFlush(windowRef.Display);
	}

	private void ModifyNetWmState(XWindowRef windowRef, ulong[] properties, bool add)
	{
		var message = new XClientMessageEvent();
		message.window = windowRef.Window;
		message.display = windowRef.Display;
		message.ptr1 = (IntPtr) (add ? 1 : 0);
		message.format = 32;
		message.message_type = XAtoms.NetWmState;
		message.type = (int) Event.ClientMessage;
		message.send_event = 1;

		if (properties.Length >= 1) message.ptr2 = (IntPtr) properties[0];
		if (properties.Length >= 2) message.ptr3 = (IntPtr) properties[1];
		if (properties.Length >= 3) message.ptr4 = (IntPtr) properties[2];
		if (properties.Length >= 4) message.ptr5 = (IntPtr) properties[3];

		var pointer = Marshal.AllocHGlobal(24 * sizeof(long));
		Marshal.StructureToPtr(message, pointer, true);
		XLib.XSendEvent(windowRef.Display, windowRef.Window, true, (long)EventMask.SubstructureNotifyMask, pointer);
		XLib.XFlush(windowRef.Display);
		Marshal.FreeHGlobal(pointer);
	}

	public void MaximizeWindow(XWindowRef windowRef)
	{
		var state = windowRef.GetULongArray(XAtoms.NetWmState);
		var alreadyMaximized = state.Any(a => a == XAtoms.NetWmStateMaximizedHorz || a == XAtoms.NetWmStateMaximizedVert);
		ModifyNetWmState(windowRef, new[] { XAtoms.NetWmStateMaximizedHorz, XAtoms.NetWmStateMaximizedVert}, !alreadyMaximized);
	}

	public void MinimizeWindow(XWindowRef windowRef)
	{
		var state = windowRef.GetULongArray(XAtoms.NetWmState);
		var alreadyIconified = state.Any(a => a == XAtoms.NetWmStateHidden);

		if (alreadyIconified)
		{
			XLib.XMapWindow(windowRef.Display, windowRef.Window);
			XLib.XRaiseWindow(windowRef.Display, windowRef.Window);
		}
		else
		{
			XLib.XIconifyWindow(windowRef.Display, windowRef.Window, 0);
		}
	}

	public void MakeWindowVisible(XWindowRef windowRef)
	{
		XLib.XMapWindow(windowRef.Display, windowRef.Window);
		XLib.XRaiseWindow(windowRef.Display, windowRef.Window);
		XLib.XFlush(windowRef.Display);
	}

	public GdkPixbufHandle CaptureWindowScreenshot(XWindowRef windowRef)
	{
		XLib.XGetWindowAttributes(windowRef.Display, windowRef.Window, out var windowAttributes);

		if (windowAttributes.map_state != XConstants.IsViewable)
		{
			return null;
		}

		var imagePointer = XLib.XGetImage(windowRef.Display, windowRef.Window, 0, 0, windowAttributes.width, windowAttributes.height, XConstants.AllPlanes, XConstants.ZPixmap);

		if (imagePointer == IntPtr.Zero)
		{
			return null;
		}

		var image = Marshal.PtrToStructure<XImage>(imagePointer);
		var imageByteLength = image.width * image.height * (image.bits_per_pixel / 8);
		var buffer = ArrayPool<byte>.Shared.Rent(imageByteLength);
		Marshal.Copy(image.data, buffer, 0, imageByteLength);
		XLib.XDestroyImage(imagePointer);

		using var fullSizeImage = GdkPixbufFactory.From(buffer, image.depth, image.width, image.height, image.bytes_per_line);
		var scaledImage = fullSizeImage.ScaleToFit(300, 300);
		ArrayPool<byte>.Shared.Return(buffer);

		return scaledImage;
	}

	public void CloseWindow(XWindowRef windowRef)
	{
		var message = new XClientMessageEvent();
		message.display = windowRef.Display;
		message.window = windowRef.Window;
		message.format = 32;
		message.type = (int) Event.ClientMessage;
		message.message_type = XAtoms.WmProtocols;
		message.send_event = 1;
		message.ptr1 = (IntPtr) XAtoms.WmDeleteWindow;
		message.ptr2 = IntPtr.Zero;

		var pointer = Marshal.AllocHGlobal(24 * sizeof(long));
		Marshal.StructureToPtr(message, pointer, true);
		XLib.XSendEvent(windowRef.Display, windowRef.Window, false, (long)EventMask.NoEventMask, pointer);
		XLib.XFlush(windowRef.Display);
		Marshal.FreeHGlobal(pointer);
	}

	public void StartResizing(XWindowRef windowRef)
	{
		XLib.XGetWindowAttributes(windowRef.Display, windowRef.Window, out var windowAttributes);
		XLib.XTranslateCoordinates(_rootWindowRef.Display, windowRef.Window, _rootWindowRef.Window, 0, 0, out var x, out var y, out _);
		XLib.XWarpPointer(windowRef.Display, 0, windowRef.Window, 0, 0, 0, 0, (int) windowAttributes.width, (int) windowAttributes.height);
		var args = new ulong[] { (ulong)(x + windowAttributes.width / 2), (ulong)(y + windowAttributes.height / 2), 4, 1, 1 };
		SendClientMessage(windowRef, XAtoms.NetWmMoveresize, args);
	}

	public void StartMoving(XWindowRef windowRef)
	{
		XLib.XGetWindowAttributes(windowRef.Display, windowRef.Window, out var windowAttributes);
		XLib.XTranslateCoordinates(_rootWindowRef.Display, windowRef.Window, _rootWindowRef.Window, 0, 0, out var x, out var y, out _);
		XLib.XWarpPointer(windowRef.Display, 0, windowRef.Window, 0, 0, 0, 0, (int) windowAttributes.width / 2, (int) windowAttributes.height / 2);
		var args = new ulong[] { (ulong)(x + windowAttributes.width / 2), (ulong)(y + windowAttributes.height / 2), 8, 1, 1 };
		SendClientMessage(windowRef, XAtoms.NetWmMoveresize, args);
	}

	private void SendClientMessage(XWindowRef windowRef, ulong messageType, ulong[] data)
	{
		var message = new XClientMessageEvent();
		message.display = windowRef.Display;
		message.window = windowRef.Window;
		message.format = 32;
		message.type = (int) Event.ClientMessage;
		message.message_type = messageType;
		message.send_event = 1;

		if (data.Length >= 1) message.ptr1 = (IntPtr) data[0];
		if (data.Length >= 2) message.ptr2 = (IntPtr) data[1];
		if (data.Length >= 3) message.ptr3 = (IntPtr) data[2];
		if (data.Length >= 4) message.ptr4 = (IntPtr) data[3];
		if (data.Length >= 5) message.ptr5 = (IntPtr) data[4];

		var pointer = Marshal.AllocHGlobal(24 * sizeof(long));
		Marshal.StructureToPtr(message, pointer, true);
		XLib.XSendEvent(windowRef.Display, windowRef.Window, true, (long)EventMask.SubstructureNotifyMask, pointer);
		XLib.XFlush(windowRef.Display);
		Marshal.FreeHGlobal(pointer);
	}
}
