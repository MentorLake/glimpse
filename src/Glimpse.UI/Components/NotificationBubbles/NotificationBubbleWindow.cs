using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.Gtk;
using Unit = System.Reactive.Unit;

namespace Glimpse.UI.Components.NotificationBubbles;

public class NotificationBubbleWindow
{
	private readonly GtkWindowHandle _root;
	private readonly NotificationBubbleContent _bubbleContent;

	public GtkWindowHandle Window => _root;
	public IObservable<string> ActionInvoked => _bubbleContent.ActionInvoked;
	public IObservable<Unit> CloseNotification => _bubbleContent.CloseNotification;

	public NotificationBubbleWindow(IObservable<NotificationBubbleViewModel> notificationStateObs)
	{
		_root = GtkWindowHandle.New(GtkWindowType.GTK_WINDOW_TOPLEVEL)
			.SetSkipPagerHint(true)
			.SetSkipTaskbarHint(true)
			.SetDecorated(false)
			.SetResizable(false)
			.SetCanFocus(false)
			.SetTypeHint(GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_NOTIFICATION)
			.Prop(w => w.SetVisual(w.GetScreen().GetRgbaVisual()))
			.SetVisible(false)
			.SetKeepAbove(true)
			.AddClass("transparent")
			.ObserveEvent(w => w.Signal_DeleteEvent(), e => e.ReturnValue = true);

		_bubbleContent = new NotificationBubbleContent(notificationStateObs);
		_root.Add(_bubbleContent.Widget);
		_root.ShowAll();
	}
}
