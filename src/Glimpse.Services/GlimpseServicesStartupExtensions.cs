using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.Microsoft.Extensions;
using Glimpse.Libraries.StatusNotifierWatcher;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg.Notifications;
using Glimpse.Services.Notifications;
using Glimpse.Services.StartMenu;
using Glimpse.Services.SystemTray;
using Glimpse.Services.Taskbar;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Glimpse.Services;

public static class GlimpseServicesStartupExtensions
{
	public static void AddServices(this IHostApplicationBuilder builder)
	{
		builder.Services.AddInstance(StartMenuReducers.AllReducers);
		builder.Services.AddSingleton<IEffectsFactory, StartMenuEffects>();

		builder.Services.AddSingleton<OrgFreedesktopNotifications>();
		builder.Services.AddSingleton<XorgNotificationsService>();
		builder.Services.AddTransient<IEffectsFactory, NotificationsEffects>();
		builder.Services.AddTransient<IReducerFactory, NotificationsReducers>();

		builder.Services.AddSingleton<TaskbarService>();
		builder.Services.AddSingleton<IEffectsFactory, TaskbarEffects>();
		builder.Services.AddInstance(TaskbarReducers.AllReducers);

		builder.Services.AddTransient<IReducerFactory, SystemTrayItemStateReducers>();
	}

	public static async Task UseServices(this IHost host, string taskbarConfigFilePath)
	{
		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();

		host.UseStartMenu(store, configurationService);
		await host.UseNotifications(store, configurationService);
		host.UseTaskbar(store, configurationService, taskbarConfigFilePath);
		await host.UseSystemTray();
	}

	private static void UseTaskbar(this IHost host, ReduxStore store, ConfigurationService configurationService, string taskbarConfigFilePath)
	{
		configurationService.AddIfNotExists(TaskbarConfiguration.ConfigKey, TaskbarConfiguration.New(taskbarConfigFilePath).ToJsonElement());

		configurationService
			.ObserveChange(TaskbarConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateTaskbarConfigurationAction(TaskbarConfiguration.From(c))));
	}

	private static async Task UseNotifications(this IHost host, ReduxStore store, ConfigurationService configurationService)
	{
		await host.Services.GetRequiredService<XorgNotificationsService>().InitializeAsync(
			host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>().Value.NotificationsFilePath);

		configurationService.AddIfNotExists(NotificationsConfiguration.ConfigKey, NotificationsConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(NotificationsConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateNotificationsConfigurationAction(NotificationsConfiguration.From(c))));
	}

	private static void UseStartMenu(this IHost host, ReduxStore store, ConfigurationService configurationService)
	{
		configurationService.AddIfNotExists(StartMenuConfiguration.ConfigKey, StartMenuConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(StartMenuConfiguration.ConfigKey)
			.SubscribeDebug(c => store.Dispatch(new UpdateStartMenuConfiguration(StartMenuConfiguration.From(c))));
	}

	private static async Task UseSystemTray(this IHost host)
	{
		await host.Services.GetRequiredService<StatusNotifierWatcherService>().InitializeAsync();

		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();
		configurationService.AddIfNotExists(SystemTrayConfiguration.ConfigKey, SystemTrayConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(SystemTrayConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateSystemTrayConfiguration(SystemTrayConfiguration.From(c))));
	}
}
