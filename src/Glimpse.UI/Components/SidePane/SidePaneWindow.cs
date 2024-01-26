using System.Reactive.Linq;
using Gdk;
using GLib;
using Glimpse.Interop.Gdk;
using MentorLake.Redux;
using Glimpse.UI.Components.SidePane.Calendar;
using Glimpse.UI.Components.SidePane.NotificationHistory;
using Glimpse.Xorg.State;
using Gtk;
using ReactiveMarbles.ObservableEvents;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace Glimpse.UI.Components.SidePane;

public class SidePaneWindow : Window
{
	private Revealer _layoutRevealer;

	public SidePaneWindow(CalendarWindow calendarWindow, NotificationHistoryWindow notificationHistoryWindow, ReduxStore store) : base(WindowType.Toplevel)
	{
		this.Events().DeleteEvent.Subscribe(e => e.RetVal = true);

		AddEvents((int) EventMask.AllEventsMask);
		SkipPagerHint = true;
		SkipTaskbarHint = true;
		Decorated = false;
		CanFocus = true;
		TypeHint = WindowTypeHint.Dialog;
		Visual = Screen.RgbaVisual;
		Visible = false;
		KeepAbove = true;
		AppPaintable = true;
		Resizable = false;

		var layout = new Box(Orientation.Vertical, 8);
		layout.Add(notificationHistoryWindow);
		layout.Add(calendarWindow);
		layout.SetSizeRequest(340, 734);
		layout.MarginEnd = 8;

		_layoutRevealer = new Revealer();
		_layoutRevealer.Child = layout;
		_layoutRevealer.TransitionType = RevealerTransitionType.SlideLeft;
		_layoutRevealer.TransitionDuration = 250;
		_layoutRevealer.RevealChild = false;
		_layoutRevealer.Halign = Align.End;
		_layoutRevealer.Valign = Align.End;

		Add(_layoutRevealer);
		SetSizeRequest(348, 734);

		store.Actions.OfType<WindowFocusedChangedAction>()
			.ObserveOn(new GLibSynchronizationContext())
			.TakeUntilDestroyed(this)
			.Where(action => IsVisible && action.WindowRef.Id != LibGdk3Interop.gdk_x11_window_get_xid(Window.Handle))
			.Subscribe(_ => ToggleVisibility());

		ShowAll();
		Hide();
	}

	public void ToggleVisibility()
	{
		Display.GetPointer(out var x, out var y);
		var eventMonitor = Display.GetMonitorAtPoint(x, y);

		if (IsVisible)
		{
			_layoutRevealer.RevealChild = false;
			Observable.Timer(TimeSpan.FromMilliseconds(250)).ObserveOn(new GLibSynchronizationContext()).Subscribe(_ => Hide());
		}
		else
		{
			Show();
			var eventPanel = Application.Windows.OfType<Panel>().First(p => eventMonitor.Contains(p.Window));
			Move(eventMonitor.Geometry.Right - Allocation.Width, eventMonitor.Geometry.Bottom - eventPanel.Window.Height - Allocation.Height - 8);
			_layoutRevealer.RevealChild = true;
		}
	}
}
