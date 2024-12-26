using System.Reactive.Linq;
using System.Text.Json;
using Glimpse.Common.DBus;
using Glimpse.Common.Gtk;
using Glimpse.Common.System.Reactive;
using Glimpse.Notifications.Components.NotificationBubbles;
using MentorLake.Redux;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Notifications;

public enum NotificationCloseReason
{
	Expired = 1,
	Dismissed = 2,
	CloseNotification = 3,
	Undefined = 4
}

public class NotificationsService(
	ILogger<NotificationsService> logger,
	OrgFreedesktopNotifications freedesktopNotifications,
	DBusConnections dBusConnections,
	OrgFreedesktopDBus orgFreedesktopDBus,
	ReduxStore store)
{
	public IObservable<NotificationBubbleWindow> Notifications { get; set; }

	public async Task InitializeAsync(string notificationsFilePath)
	{
		dBusConnections.Session.AddMethodHandler(freedesktopNotifications);
		await orgFreedesktopDBus.RequestNameAsync("org.freedesktop.Notifications", 0);

		freedesktopNotifications.CloseNotificationRequested.Subscribe(id =>
		{
			freedesktopNotifications.EmitNotificationClosed(id, (int) NotificationCloseReason.CloseNotification);
			store.Dispatch(new CloseNotificationAction(id));
		});

		freedesktopNotifications.Notifications.Subscribe(n =>
		{
			store.Dispatch(new AddNotificationAction(n));
		});

		store
			.Select(NotificationSelectors.NotificationsState)
			.Select(s => s.ById.Values)
			.UnbundleMany(n => n.Id)
			.RemoveIndex()
			.Subscribe(obs =>
			{
				obs.Take(1).Subscribe(n =>
				{
					Observable.Timer(n.Duration)
						.TakeUntil(obs.TakeLast(1))
						.Take(1)
						.Subscribe(_ =>
						{
							freedesktopNotifications.EmitNotificationClosed(n.Id, (int) NotificationCloseReason.Expired);
							store.Dispatch(new NotificationTimerExpiredAction(n.Id));
						});
				});
		});

		if (File.Exists(notificationsFilePath))
		{
			var historyJson = await File.ReadAllTextAsync(notificationsFilePath);
			var history = JsonSerializer.Deserialize(historyJson, typeof(NotificationHistory), NotificationsJsonSerializer.Instance) as NotificationHistory;
			await store.Dispatch(new LoadNotificationHistoryAction(history));
		}

		store
			.Select(NotificationSelectors.NotificationHistory)
			.Subscribe(history =>
			{
				try
				{
					var directory = Path.GetDirectoryName(notificationsFilePath);
					if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
					File.WriteAllText(notificationsFilePath, JsonSerializer.Serialize(history, typeof(NotificationHistory), NotificationsJsonSerializer.Instance));
				}
				catch (Exception e)
				{
					logger.LogError(e.ToString());
				}
			});

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
						.Subscribe(_ => DismissNotification(notificationObservable.Key.Id));

					newWindow.ActionInvoked
						.TakeUntil(notificationObservable.TakeLast(1))
						.Subscribe(action => ActionInvoked(notificationObservable.Key.Id, action));

					notificationObservable
						.TakeLast(1)
						.ObserveOn(GLibExt.Scheduler)
						.Subscribe(_ => newWindow.Dispose());

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

	public void RemoveHistoryItem(Guid id)
	{
		store.Dispatch(new RemoveHistoryItemAction(id));
	}

	private void ActionInvoked(uint notificationId, string action)
	{
		freedesktopNotifications.EmitActionInvoked(notificationId, action);
	}

	private void DismissNotification(uint notificationId)
	{
		freedesktopNotifications.EmitNotificationClosed(notificationId, (int) NotificationCloseReason.Dismissed);
		store.Dispatch(new CloseNotificationAction(notificationId));
	}

	public void ClearHistory()
	{
		store.Dispatch(new ClearNotificationHistoryAction());
	}

	public void RemoveHistoryForApplication(string appName)
	{
		store.Dispatch(new RemoveHistoryForApplicationAction(appName));
	}
}
