using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Services.Notifications;
using MentorLake.Gtk;
using MentorLake.Redux;
using Microsoft.Extensions.Logging;

namespace Glimpse.UI.Components.NotificationBubbles;

public class NotificationBubblesService(
	ReduxStore store,
	ILogger<NotificationBubblesService> logger,
	XorgNotificationsService service)
{
	public IObservable<NotificationBubbleWindow> Notifications { get; set; }

	public void Initialize()
	{
		Notifications = store
			.Select(NotificationBubbleSelectors.ViewModel)
			.ObserveOn(GLibExt.Scheduler)
			.Select(vm => vm.Notifications)
			.UnbundleMany(n => n.Id)
			.Select(notificationObservable =>
			{
				try
				{
					var newWindow = new NotificationBubbleWindow(notificationObservable.Select(x => x.Item1));

					newWindow.CloseNotification
						.TakeUntil(notificationObservable.TakeLast(1))
						.Subscribe(_ => service.DismissNotification(notificationObservable.Key.Id));

					newWindow.ActionInvoked
						.TakeUntil(notificationObservable.TakeLast(1))
						.Subscribe(action => service.InvokeAction(notificationObservable.Key.Id, action));

					notificationObservable
						.TakeLast(1)
						.ObserveOn(GLibExt.Scheduler)
						.Subscribe(_ => newWindow.Window.Destroy());

					return newWindow;
				}
				catch (Exception e)
				{
					logger.LogError(e.ToString());
					throw;
				}
			})
			.Publish()
			.AutoConnect();
	}
}
