using Glimpse.Common.Microsoft.Extensions;
using Glimpse.UI.Components.NotificationsConfig;
using Glimpse.UI.Components.SidePane.NotificationHistory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Freedesktop.Notifications;

public static class NotificationsStartupExtensions
{
	public static async Task UseNotifications(this IHost host)
	{
		var container = host.Services;
		await container.GetRequiredService<NotificationsService>().InitializeAsync();
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
