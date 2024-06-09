using Glimpse.Common.Configuration;
using Glimpse.Notifications.Components.NotificationHistory;
using Glimpse.Notifications.Components.NotificationsConfig;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Glimpse.Notifications;

public static class NotificationsStartupExtensions
{
	public static async Task UseNotifications(this IHost host)
	{
		await host.Services.GetRequiredService<NotificationsService>().InitializeAsync(
			host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>().Value.NotificationsFilePath);

		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();
		configurationService.AddIfNotExists(NotificationsConfiguration.ConfigKey, NotificationsConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(NotificationsConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateNotificationsConfigurationAction(NotificationsConfiguration.From(c))));
	}

	public static void AddNotifications(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<OrgFreedesktopNotifications>();
		builder.Services.AddSingleton<NotificationsService>();
		builder.Services.AddSingleton<NotificationHistoryWindow>();
		builder.Services.AddSingleton<NotificationsConfigWidget>();
		builder.Services.AddSingleton<NotificationsConfigWindow>();
		builder.Services.AddTransient<IEffectsFactory, NotificationsEffects>();
		builder.Services.AddTransient<IReducerFactory, NotificationsReducers>();
	}
}
