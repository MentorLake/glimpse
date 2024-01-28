using Glimpse.Common.Configuration;
using Glimpse.Common.StatusNotifierWatcher;
using Glimpse.SystemTray.Components;
using MentorLake.Redux;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.SystemTray;

public static class SystemTrayStartupExtensions
{
	public static async Task UseSystemTray(this IHost host)
	{
		await host.Services.GetRequiredService<StatusNotifierWatcherService>().InitializeAsync();

		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();
		configurationService.AddIfNotExists(SystemTrayConfiguration.ConfigKey, SystemTrayConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(SystemTrayConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateSystemTrayConfiguration(SystemTrayConfiguration.From(c))));
	}

	public static void AddSystemTray(this IHostApplicationBuilder builder)
	{
		builder.Services.AddTransient<IReducerFactory, SystemTrayItemStateReducers>();
		builder.Services.AddTransient<SystemTrayBox>();
	}
}
