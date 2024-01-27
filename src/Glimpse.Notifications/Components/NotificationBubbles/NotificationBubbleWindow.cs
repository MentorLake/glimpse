using Gdk;
using ReactiveMarbles.ObservableEvents;
using Unit = System.Reactive.Unit;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace Glimpse.Notifications.Components.NotificationBubbles;

public class NotificationBubbleWindow : Window
{
	private readonly NotificationBubbleContent _bubbleContent;

	public NotificationBubbleWindow(IObservable<NotificationBubbleViewModel> notificationStateObs) : base(WindowType.Toplevel)
	{
		SkipPagerHint = true;
		SkipTaskbarHint = true;
		Decorated = false;
		Resizable = false;
		CanFocus = false;
		TypeHint = WindowTypeHint.Notification;
		Visual = Screen.RgbaVisual;
		AppPaintable = true;
		Visible = false;
		KeepAbove = true;

		this.Events().DeleteEvent.Subscribe(e => e.RetVal = true);

		_bubbleContent = new NotificationBubbleContent(notificationStateObs);
		Add(_bubbleContent);
		ShowAll();
	}

	public IObservable<string> ActionInvoked => _bubbleContent.ActionInvoked;
	public IObservable<Unit> CloseNotification => _bubbleContent.CloseNotification;
}
