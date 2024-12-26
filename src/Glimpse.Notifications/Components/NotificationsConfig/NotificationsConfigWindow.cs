using Gdk;
using Glimpse.Common.Gtk;
using ReactiveMarbles.ObservableEvents;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace Glimpse.Notifications.Components.NotificationsConfig;

public class NotificationsConfigWindow : Window
{
	public NotificationsConfigWindow(NotificationsConfigWidget widget) : base(WindowType.Toplevel)
	{
		Title = "Notifications Config";
		CanFocus = true;
		TypeHint = WindowTypeHint.Normal;
		Visible = false;
		Resizable = false;

		this.ObserveEvent(w => w.Events().DeleteEvent).Subscribe(e =>
		{
			e.RetVal = true;
			Hide();
		});

		Add(widget);
		SetSizeRequest(400, 0);
		widget.ShowAll();
		Hide();
	}

	public void ShowAndCenterOnScreen()
	{
		Display.GetPointer(out var x, out var y);
		var eventMonitor = Display.GetMonitorAtPoint(x, y);
		Present();
		Move(eventMonitor.Geometry.Left + eventMonitor.Geometry.Width / 2 - Allocation.Width / 2, eventMonitor.Geometry.Top + eventMonitor.Geometry.Height / 2 - Allocation.Height / 2);
	}
}
