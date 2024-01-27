using Glimpse.Common.Configuration;
using Glimpse.SystemTray.Components;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.SystemTray;

public static class SystemTrayStartupExtensions
{
	public static async Task UseSystemTray(this IHost host)
	{
		await host.Services.GetRequiredService<DBusSystemTrayService>().InitializeAsync();

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
		builder.Services.AddSingleton<IEffectsFactory, SystemTrayItemStateEffects>();
		builder.Services.AddTransient<DBusSystemTrayService>();
		builder.Services.AddTransient<SystemTrayBox>();
	}
}
