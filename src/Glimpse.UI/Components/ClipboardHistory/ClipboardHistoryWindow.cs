using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Services.Clipboard;
using MentorLake.GdkX11;
using MentorLake.GObject;

namespace Glimpse.UI.Components.ClipboardHistory;

public class ClipboardHistoryWindow
{
	private const int PanelWidth = 340;
	private const int DefaultPanelHeight = 734;
	private const int EdgeMargin = 8;
	private const int MaxPreviewChars = 150;

	private readonly GtkListBoxHandle _listBox;
	private readonly GtkRevealerHandle _revealer;
	private readonly GtkWindowHandle _root;
	private readonly ReduxStore _store;
	private readonly IDisplayServer _displayServer;
	private readonly Dictionary<Guid, GtkListBoxRowHandle> _rows = new();
	private readonly List<Guid> _displayOrder = new();
	private string? _pendingPasteText;

	public GtkWindowHandle Window => _root;

	public ClipboardHistoryWindow(ReduxStore store, IDisplayServer displayServer)
	{
		_store = store;
		_displayServer = displayServer;

		_root = GtkWindowHandle.New(GtkWindowType.GTK_WINDOW_TOPLEVEL)
			.ObserveEvent(w => w.Signal_DeleteEvent(), e => e.ReturnValue = true)
			.AddEvents((int) GdkEventMask.GDK_ALL_EVENTS_MASK)
			.SetSkipPagerHint(true)
			.SetSkipTaskbarHint(true)
			.SetDecorated(false)
			.SetCanFocus(true)
			.SetTypeHint(GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_DIALOG)
			.Prop(w => w.SetVisual(w.GetScreen().GetRgbaVisual()))
			.SetVisible(false)
			.SetKeepAbove(true)
			.SetResizable(false)
			.AddClass("transparent");

		_listBox = GtkListBoxHandle.New()
			.SetSelectionMode(GtkSelectionMode.GTK_SELECTION_SINGLE)
			.AddClass("clipboard-history__list");

		_listBox.Signal_RowActivated(GConnectFlags.G_CONNECT_DEFAULT)
			.SubscribeDebug(signal => Paste(_displayOrder[signal.Row.GetIndex()]));

		var scrolledWindow = GtkScrolledWindowHandle.New(null, null)
			.SetPolicy(GtkPolicyType.GTK_POLICY_NEVER, GtkPolicyType.GTK_POLICY_AUTOMATIC)
			.SetOverlayScrolling(true)
			.SetVexpand(true)
			.AddClass("clipboard-history__scroller")
			.AddMany(_listBox);

		var clearAllButton = GtkButtonHandle.New()
			.AddButtonStates()
			.AddClass("clipboard-history__clear-button")
			.SetTooltipText("Clear unpinned entries")
			.AddMany(GtkImageHandle.New()
				.SetFromIconName("edit-clear-all-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR)
				.SetPixelSize(16))
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), e =>
			{
				e.ReturnValue = true;
				_store.Dispatch(new ClearClipboardHistoryAction());
			});

		var header = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 4)
			.AddClass("clipboard-history__header-row")
			.AddMany(GtkLabelHandle.New("Clipboard history")
				.AddClass("clipboard-history__header")
				.SetXalign(0)
				.SetHexpand(true))
			.AddMany(clearAllButton);

		var layout = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
			.AddClass("clipboard-history__container")
			.AddMany(header)
			.AddMany(scrolledWindow)
			.SetSizeRequest(PanelWidth, DefaultPanelHeight)
			.SetMarginEnd(EdgeMargin);

		_revealer = GtkRevealerHandle.New()
			.Add(layout)
			.SetTransitionType(GtkRevealerTransitionType.GTK_REVEALER_TRANSITION_TYPE_SLIDE_RIGHT)
			.SetTransitionDuration(250)
			.SetRevealChild(false)
			.SetHalign(GtkAlign.GTK_ALIGN_START)
			.SetValign(GtkAlign.GTK_ALIGN_FILL);

		_root.Add(_revealer);
		_root.SetSizeRequest(PanelWidth + EdgeMargin, DefaultPanelHeight);
		_root.ObserveEvent(w => w.Signal_KeyPressEvent(GConnectFlags.G_CONNECT_DEFAULT), OnKeyPressEvent);

		store
			.Select(ClipboardSelectors.Entries)
			.ConflateLatest(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.SubscribeDebug(RebuildRows);

		store.Actions
			.OfType<ClipboardPickerOpenedAction>()
			.ConflateLatest(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.SubscribeDebug(_ => ToggleVisibility());

		store.Actions
			.OfType<WindowFocusedChangedAction>()
			.ConflateLatest(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.Where(action => action.WindowRef.Id != _root.GetWindow().GetXid().Value)
			.SubscribeDebug(_ =>
			{
				if (_root.IsVisible())
				{
					HideInternal();
					_pendingPasteText = null;
				}
				else if (_pendingPasteText != null)
				{
					_displayServer.PasteIntoFocusedWindow(_pendingPasteText);
					_pendingPasteText = null;
				}
			});

		_root.ShowAll();
		_root.Hide();
	}

	private void RebuildRows(ImmutableList<ClipboardEntry> entries)
	{
		foreach (var row in _rows.Values)
		{
			row.Destroy();
		}

		_rows.Clear();
		_displayOrder.Clear();

		var sorted = entries.OrderBy(e => e.IsPinned ? 0 : 1);

		foreach (var entry in sorted)
		{
			var row = CreateRow(entry);
			_listBox.Insert(row, -1);
			_rows[entry.Id] = row;
			_displayOrder.Add(entry.Id);
		}

		_listBox.ShowAll();

		if (_displayOrder.Count > 0)
		{
			_listBox.SelectRow(_rows[_displayOrder[0]]);
		}
	}

	private GtkListBoxRowHandle CreateRow(ClipboardEntry entry)
	{
		var preview = GtkLabelHandle.New(TruncateText(entry.Text))
			.AddClass("clipboard-history-item__text")
			.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END)
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.SetXalign(0)
			.SetYalign(0);

		var timestamp = GtkLabelHandle.New(FormatRelativeTime(entry.CapturedAt))
			.AddClass("clipboard-history-item__timestamp")
			.SetXalign(0);

		var pinButton = GtkButtonHandle.New()
			.AddButtonStates()
			.AddClass("clipboard-history-item__pin-button")
			.AddClass(entry.IsPinned ? "clipboard-history-item__pin-button--pinned" : "")
			.AddMany(GtkImageHandle.New()
				.SetFromIconName(entry.IsPinned ? "starred-symbolic" : "non-starred-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR)
				.SetPixelSize(16))
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), e =>
			{
				e.ReturnValue = true;
				_store.Dispatch(new PinClipboardItemAction(entry.Id));
			});

		var closeButton = GtkButtonHandle.New()
			.AddButtonStates()
			.AddClass("clipboard-history-item__delete-button")
			.AddMany(GtkImageHandle.New()
				.SetFromIconName("window-close-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR)
				.SetPixelSize(16))
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), e =>
			{
				e.ReturnValue = true;
				_store.Dispatch(new RemoveClipboardItemAction(entry.Id));
			});

		var content = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 2)
			.AddClass("clipboard-history-item__container")
			.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 4)
				.AddMany(timestamp)
				.AddMany(pinButton)
				.AddMany(closeButton)
				.SetHalign(GtkAlign.GTK_ALIGN_FILL))
			.AddMany(preview);

		var row = GtkListBoxRowHandle.New()
			.AddClass("clipboard-history-item")
			.ObserveEvent(
				w => w.Signal_ButtonReleaseEvent(),
				e => Paste(entry.Id))
			.AddMany(content);

		return row;
	}

	private void OnKeyPressEvent(GtkWidgetHandleSignalStructs.KeyPressEventSignal args)
	{
		var e = args.Event.Dereference();

		if (e.keyval == GdkConstants.KEY_Escape)
		{
			HideInternal();
			args.ReturnValue = true;
			return;
		}

		if (e.keyval == GdkConstants.KEY_Return)
		{
			var selectedRow = _listBox.GetSelectedRow();
			if (selectedRow != null)
			{
				Paste(_displayOrder[selectedRow.GetIndex()]);
			}
			args.ReturnValue = true;
			return;
		}

		if (e.keyval == GdkConstants.KEY_Down)
		{
			MoveSelection(1);
			args.ReturnValue = true;
			return;
		}

		if (e.keyval == GdkConstants.KEY_Up)
		{
			MoveSelection(-1);
			args.ReturnValue = true;
		}
	}

	private void MoveSelection(int delta)
	{
		var selected = _listBox.GetSelectedRow();
		var selectedIndex = selected == null ? -1 : selected.GetIndex();
		var targetIndex = Math.Clamp(selectedIndex + delta, 0, _rows.Count - 1);
		var targetRow = _listBox.GetRowAtIndex(targetIndex);

		if (targetRow != null)
		{
			_listBox.SelectRow(targetRow);
		}
	}

	private void Paste(Guid entryId)
	{
		var entry = ClipboardSelectors.Entries
			.Apply(_store.State)
			.FirstOrDefault(e => e.Id == entryId);

		if (entry != null)
		{
			_pendingPasteText = entry.Text;
		}

		HideInternal();
	}

	private void HideInternal()
	{
		_revealer.SetRevealChild(false);
		Observable.Timer(TimeSpan.FromMilliseconds(_revealer.GetTransitionDuration())).ConflateLatest(GLibExt.Scheduler).SubscribeDebug(_ => _root.Hide());
	}

	private void ToggleVisibility()
	{
		if (_root.IsVisible())
		{
			HideInternal();
			return;
		}

		_root.GetDisplay().GetPointer(out _, out var pointerX, out var pointerY, out _);
		var workArea = _root.GetDisplay().GetMonitorAtPoint(pointerX, pointerY).GetWorkAreaRect();

		_root.SetSizeRequest(PanelWidth + EdgeMargin, workArea.Height);
		_root.Show();
		_root.Move(workArea.Left, workArea.Top);
		_revealer.SetRevealChild(true);
		_root.GrabFocus();
	}

	private static string TruncateText(string text)
	{
		if (text.Length <= MaxPreviewChars) return text;
		return text[..MaxPreviewChars] + "…";
	}

	private static string FormatRelativeTime(DateTime capturedAt)
	{
		var elapsed = DateTime.UtcNow - capturedAt;
		if (elapsed < TimeSpan.FromMinutes(1)) return "just now";
		if (elapsed < TimeSpan.FromHours(1)) return $"{(int) elapsed.TotalMinutes}m ago";
		if (elapsed < TimeSpan.FromDays(1)) return $"{(int) elapsed.TotalHours}h ago";
		return $"{(int) elapsed.TotalDays}d ago";
	}
}
