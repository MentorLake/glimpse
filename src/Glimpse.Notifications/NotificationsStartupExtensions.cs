using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Configuration;
using Glimpse.UI.Components.NotificationsConfig;
using Glimpse.UI.Components.SidePane.NotificationHistory;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Freedesktop.Notifications;

public static class NotificationsStartupExtensions
{
	public static async Task UseNotifications(this IHost host)
	{
		await host.Services.GetRequiredService<NotificationsService>().InitializeAsync();

		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();
		configurationService.AddIfNotExists(NotificationsConfiguration.ConfigKey, NotificationsConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(NotificationsConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateNotificationsConfigurationAction(NotificationsConfiguration.From(c))));
	}

	public static void AddNotifications(this IHostApplicationBuilder builder)
	{
		builder.Services.AddInstance(NotificationsReducers.AllReducers);
		builder.Services.AddSingleton<OrgFreedesktopNotifications>();
		builder.Services.AddSingleton<NotificationsService>();
		builder.Services.AddSingleton<NotificationHistoryWindow>();
		builder.Services.AddSingleton<NotificationsConfigWidget>();
		builder.Services.AddSingleton<NotificationsConfigWindow>();
	}
}
