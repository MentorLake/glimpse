using System.Reactive.Linq;
using System.Text.Json;
using Glimpse.Common.DBus;
using Glimpse.Common.System.Reactive;
using MentorLake.Redux;
using Microsoft.Extensions.Logging;

namespace Glimpse.Notifications;

public enum NotificationCloseReason : int
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
	}

	public void RemoveHistoryItem(Guid id)
	{
		store.Dispatch(new RemoveHistoryItemAction(id));
	}

	public void ActionInvoked(uint notificationId, string action)
	{
		freedesktopNotifications.EmitActionInvoked(notificationId, action);
	}

	public void DismissNotification(uint notificationId)
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
