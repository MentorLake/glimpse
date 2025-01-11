using Glimpse.UI.Components.ApplicationIcons;
using Glimpse.UI.Components.Calendar;
using Glimpse.UI.Components.NotificationBubbles;
using Glimpse.UI.Components.NotificationHistory;
using Glimpse.UI.Components.NotificationsConfig;
using Glimpse.UI.Components.Panel;
using Glimpse.UI.Components.SidePane;
using Glimpse.UI.Components.StartMenu;
using Glimpse.UI.Components.StartMenuIcon;
using Glimpse.UI.Components.SystemTray;
using MentorLake.Gio;
using MentorLake.Gtk;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.UI;

public static class UIStartupExtensions
{
	public static Task UseUI(this IHost host)
	{
		host.Services.GetRequiredService<NotificationBubblesService>().Initialize();

		var orchestrator = host.Services.GetRequiredService<DisplayOrchestrator>();
		orchestrator.Init();
		orchestrator.WatchNotifications();

		return Task.CompletedTask;
	}

	public static void AddUI(this IHostApplicationBuilder builder, string gtkApplicationIdentifier)
	{
		builder.Services.AddSingleton<NotificationBubblesService>();
		builder.Services.AddSingleton<NotificationHistoryWindow>();
		builder.Services.AddSingleton<NotificationsConfigWidget>();
		builder.Services.AddSingleton<NotificationsConfigWindow>();
		builder.Services.AddSingleton<CalendarWindow>();
		builder.Services.AddSingleton<SidePaneWindow>();
		builder.Services.AddTransient<Panel>();
		builder.Services.AddTransient<StartMenuIcon>();
		builder.Services.AddTransient<TaskbarView>();
		builder.Services.AddSingleton<StartMenuWindow>();
		builder.Services.AddTransient<SystemTrayBox>();

		builder.Services.AddSingleton(_ => GtkApplicationHandle.New(gtkApplicationIdentifier, GApplicationFlags.G_APPLICATION_FLAGS_NONE));
		builder.Services.AddHostedService<GlimpseGtkApplication>();
		builder.Services.AddSingleton<GlimpseGtkApplication>();
		builder.Services.AddSingleton<DisplayOrchestrator>();
		builder.Services.AddSingleton<IReducerFactory, GlimpseGtkReducerFactory>();
	}
}
