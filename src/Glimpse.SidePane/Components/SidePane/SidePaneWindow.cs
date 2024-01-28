using System.Reactive.Linq;
using Gdk;
using GLib;
using Glimpse.Common.Gtk;
using Glimpse.Common.Xorg.State;
using Glimpse.Notifications.Components.NotificationHistory;
using Glimpse.SidePane.Components.Calendar;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace Glimpse.SidePane.Components.SidePane;

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
			.Subscribe(_ => HideInternal());

		ShowAll();
		Hide();
	}

	private void HideInternal()
	{
		_layoutRevealer.RevealChild = false;
		Observable.Timer(TimeSpan.FromMilliseconds(_layoutRevealer.TransitionDuration)).ObserveOn(new GLibSynchronizationContext()).Subscribe(_ => Hide());
	}

	public void ToggleVisibility(int right, int bottom)
	{
		if (IsVisible)
		{
			HideInternal();
		}
		else
		{
			Show();
			Move(right - Allocation.Width, bottom - Allocation.Height - 8);
			_layoutRevealer.RevealChild = true;
		}
	}
}
