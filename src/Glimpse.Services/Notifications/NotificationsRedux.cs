using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.Images;
using Glimpse.Libraries.Xorg.Notifications;
using MentorLake.GdkPixbuf;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Services.Notifications;

public record UpdateNotificationsConfigurationAction(NotificationsConfiguration Config);
public record LoadNotificationHistoryAction(NotificationHistory History);
public record AddNotificationAction(FreedesktopNotification Notification);
public record NotificationTimerExpiredAction(uint NotificationId);
public record CloseNotificationAction(uint NotificationId);
public record RemoveHistoryItemAction(Guid Id);
public record ClearNotificationHistoryAction();
public record RemoveHistoryForApplicationAction(string AppName);
public record UpdateShowInPopupsAction(string AppName, bool Active);
public record UpdateShowInHistoryAction(string AppName, bool Active);

public record NotificationHistoryApplication
{
	public string Name { get; set; }
	public string DesktopEntry { get; set; }
	public string Icon { get; set; }
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

	[JsonConverter(typeof(GdkPixbufHandleJsonConverter))]
	public GdkPixbufHandle Image { get; set; }
}

public static class NotificationSelectors
{
	public static readonly ISelector<NotificationsConfiguration> NotificationsConfiguration = SelectorFactory.CreateFeature<NotificationsConfiguration>();
	public static readonly ISelector<NotificationHistory> NotificationHistory = SelectorFactory.CreateFeature<NotificationHistory>();
	public static readonly ISelector<DataTable<uint, FreedesktopNotification>> NotificationsState = SelectorFactory.CreateFeature<DataTable<uint, FreedesktopNotification>>();
	public static readonly ISelector<ImmutableList<NotificationHistoryApplication>> KnownApplications = SelectorFactory.Create(NotificationHistory, history => history.KnownApplications);
}

internal class NotificationsReducers : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new NotificationsConfiguration())
			.On<UpdateNotificationsConfigurationAction>((s, a) => a.Config)
			.On<UpdateShowInPopupsAction>((s, a) =>
			{
				var appConfig = s.Applications.First(app => app.Name == a.AppName);
				var newAppConfigList = s.Applications.Replace(appConfig, appConfig with { ShowPopupBubbles = a.Active });
				return s with { Applications = newAppConfigList };
			})
			.On<UpdateShowInHistoryAction>((s, a) =>
			{
				var appConfig = s.Applications.First(app => app.Name == a.AppName);
				var newAppConfigList = s.Applications.Replace(appConfig, appConfig with { ShowInHistory = a.Active });
				return s with { Applications = newAppConfigList };
			})
			.On<AddNotificationAction>((s, a) => s.Applications.Any(app => app.Name == a.Notification.AppName)
				? s
				: s with { Applications = s.Applications.Add(new() { Name = a.Notification.AppName }) }),
		FeatureReducer.Build(new NotificationHistory())
			.On<LoadNotificationHistoryAction>((s, a) => a.History)
			.On<RemoveHistoryForApplicationAction>((s, a) => s with { Notifications = s.Notifications.Where(n => n.AppName != a.AppName).ToImmutableList() })
			.On<ClearNotificationHistoryAction>((s, a) => s with { Notifications = ImmutableList<NotificationHistoryEntry>.Empty })
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
					var app = new NotificationHistoryApplication() { Name = a.Notification.AppName, DesktopEntry = a.Notification.DesktopEntry, Icon = a.Notification.AppIcon };
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

internal class NotificationsEffects(ReduxStore store, ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.Create(actions => actions
			.Where(a => a is AddNotificationAction or UpdateShowInPopupsAction or UpdateShowInHistoryAction)
			.WithLatestFrom(store.Select(NotificationSelectors.NotificationsConfiguration))
			.Do(t =>
			{
				var (_, config) = t;
				var serializedConfig = JsonSerializer.SerializeToNode(config, typeof(NotificationsConfiguration), NotificationsJsonSerializer.Instance)?.AsObject();
				configurationService.Upsert(NotificationsConfiguration.ConfigKey, serializedConfig);
			}))
	};
}
