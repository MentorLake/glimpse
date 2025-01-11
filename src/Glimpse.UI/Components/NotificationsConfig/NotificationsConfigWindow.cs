using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.NotificationsConfig;

public class NotificationsConfigWindow
{
	private readonly GtkWindowHandle _root;

	public GtkWindowHandle Window => _root;

	public NotificationsConfigWindow(NotificationsConfigWidget widget)
	{
		widget.Widget.ShowAll();

		_root = GtkWindowHandle.New(GtkWindowType.GTK_WINDOW_TOPLEVEL)
			.SetTitle("Notifications Config")
			.SetCanFocus(true)
			.SetTypeHint(GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_NORMAL)
			.SetVisible(false)
			.SetResizable(false)
			.ObserveEvent(w => w.Signal_DeleteEvent(), e =>
			{
				e.ReturnValue = true;
				e.Self.Hide();
			})
			.Add(widget.Widget)
			.SetSizeRequest(400, 0)
			.Hide();
	}

	public void ShowAndCenterOnScreen()
	{
		_root.GetDisplay().GetPointer(out _, out var x, out var y, out _);
		var eventMonitor = _root.GetDisplay().GetMonitorAtPoint(x, y);
		_root.Present();
		var eventMonitorGeometry = eventMonitor.GetGeometryRect();
		var windowAllocation = _root.GetAllocationRect();
		_root.Move(eventMonitorGeometry.Left + eventMonitorGeometry.Width / 2 - windowAllocation.Width / 2, eventMonitorGeometry.Top + eventMonitorGeometry.Height / 2 - windowAllocation.Height / 2);
	}
}
