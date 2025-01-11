using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.Xorg.State;
using Glimpse.UI.Components.Calendar;
using Glimpse.UI.Components.NotificationHistory;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.GdkX11;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.UI.Components.SidePane;

public class SidePaneWindow
{
	private readonly GtkRevealerHandle _layoutRevealer;
	private readonly GtkWindowHandle _root;

	public GtkWindowHandle Window => _root;

	public SidePaneWindow(CalendarWindow calendarWindow, NotificationHistoryWindow notificationHistoryWindow, ReduxStore store)
	{
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

		var layout = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 8)
			.Add(notificationHistoryWindow.Widget)
			.Add(calendarWindow.Widget)
			.SetSizeRequest(340, 734)
			.SetMarginEnd(8);

		_layoutRevealer = GtkRevealerHandle.New()
			.Add(layout)
			.SetTransitionType(GtkRevealerTransitionType.GTK_REVEALER_TRANSITION_TYPE_SLIDE_LEFT)
			.SetTransitionDuration(250)
			.SetRevealChild(false)
			.SetHalign(GtkAlign.GTK_ALIGN_END)
			.SetValign(GtkAlign.GTK_ALIGN_END);

		_root.Add(_layoutRevealer);
		_root.SetSizeRequest(348, 734);

		store.Actions.OfType<WindowFocusedChangedAction>()
			.ObserveOn(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.Where(action => _root.IsVisible() && action.WindowRef.Id != _root.GetWindow().GetXid().Value)
			.Subscribe(_ => HideInternal());

		_root.ShowAll();
		_root.Hide();
	}

	private void HideInternal()
	{
		_layoutRevealer.SetRevealChild(false);
		Observable.Timer(TimeSpan.FromMilliseconds(_layoutRevealer.GetTransitionDuration())).ObserveOn(GLibExt.Scheduler).Subscribe(_ => _root.Hide());
	}

	public void ToggleVisibility(int right, int bottom)
	{
		if (_root.IsVisible())
		{
			HideInternal();
		}
		else
		{
			_root.Show();
			var rootAllocation = _root.GetAllocationRect();
			_root.Move(right - rootAllocation.Width, bottom - rootAllocation.Height - 8);
			_layoutRevealer.SetRevealChild(true);
		}
	}
}
