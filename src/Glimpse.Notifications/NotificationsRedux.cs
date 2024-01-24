using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Glimpse.Common.Images;
using Glimpse.Configuration;
using MentorLake.Redux;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Freedesktop.Notifications;

public record UpdateNotificationsConfigurationAction(NotificationsConfiguration Config);
public record LoadNotificationHistoryAction(NotificationHistory History);
public record AddNotificationAction(FreedesktopNotification Notification);
public record NotificationTimerExpiredAction(uint NotificationId);
public record CloseNotificationAction(uint NotificationId);
public record RemoveHistoryItemAction(Guid Id);
public record ClearNotificationHistory();
public record RemoveHistoryForApplicationAction(string AppName);
public record NotificationHistoryApplication
{
	public string Name { get; set; }
	public string DesktopEntry { get; set; }
}

public record NotificationHistory
{
	public ImmutableList<NotificationHistoryEntry> Notifications { get; set; } = ImmutableList<NotificationHistoryEntry>.Empty;
	public ImmutableList<NotificationHistoryApplication> KnownApplications { get; set; } = ImmutableList<NotificationHistoryApplication>.Empty;
}

public record NotificationHistoryEntry
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string AppName { get; set; }
	public string AppIcon { get; set; }
	public string Summary { get; set; }
	public string Body { get; set; }
	public DateTime CreationDate { get; set; }
	public string DesktopEntry { get; set; }
	public string ImagePath { get; set; }

	[JsonConverter(typeof(GlimpseImageJsonConverter))]
	public IGlimpseImage Image { get; set; }
}

public static class NotificationSelectors
{
	public static readonly ISelector<NotificationsConfiguration> NotificationsConfiguration = SelectorFactory.CreateFeature<NotificationsConfiguration>();
	public static readonly ISelector<NotificationHistory> NotificationHistory = SelectorFactory.CreateFeature<NotificationHistory>();
	public static readonly ISelector<DataTable<uint, FreedesktopNotification>> NotificationsState = SelectorFactory.CreateFeature<DataTable<uint, FreedesktopNotification>>();
	public static readonly ISelector<ImmutableList<NotificationHistoryApplication>> KnownApplications = SelectorFactory.Create(NotificationHistory, history => history.KnownApplications);
}

internal static class NotificationsReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new NotificationsConfiguration())
			.On<UpdateNotificationsConfigurationAction>((s, a) => a.Config),
		FeatureReducer.Build(new NotificationHistory())

			.On<LoadNotificationHistoryAction>((s, a) => a.History)
			.On<RemoveHistoryForApplicationAction>((s, a) => s with { Notifications = s.Notifications.Where(n => n.AppName != a.AppName).ToImmutableList() })
			.On<ClearNotificationHistory>((s, a) => s with { Notifications = ImmutableList<NotificationHistoryEntry>.Empty })
			.On<RemoveHistoryItemAction>((s, a) => s with { Notifications = s.Notifications.Where(n => n.Id != a.Id).ToImmutableList() })
			.On<AddNotificationAction>((s, a) =>
			{
				var result = s;

				var notificationHistoryEntry = new NotificationHistoryEntry()
				{
					AppName = a.Notification.AppName,
					AppIcon = a.Notification.AppIcon,
					CreationDate = a.Notification.CreationDate,
					Body = a.Notification.Body,
					Summary = a.Notification.Summary,
					DesktopEntry = a.Notification.DesktopEntry,
					ImagePath = a.Notification.ImagePath,
					Image = a.Notification.Image?.ScaleToFit(34, 34)
				};

				if (result.KnownApplications.All(app => app.Name != a.Notification.AppName))
				{
					var app = new NotificationHistoryApplication() { Name = a.Notification.AppName, DesktopEntry = a.Notification.DesktopEntry };
					result = result with { KnownApplications = s.KnownApplications.Add(app) };
				}

				return result with
				{
					Notifications = result.Notifications
						.Where(x => x.CreationDate.AddDays(3) > DateTime.UtcNow)
						.Concat([notificationHistoryEntry])
						.ToImmutableList()
				};
			}),
		FeatureReducer.Build(new DataTable<uint, FreedesktopNotification>())
			.On<AddNotificationAction>((s, a) => s.UpsertOne(a.Notification))
			.On<NotificationTimerExpiredAction>((s, a) => s.Remove(a.NotificationId))
			.On<CloseNotificationAction>((s, a) => s.Remove(a.NotificationId))
	];
}
