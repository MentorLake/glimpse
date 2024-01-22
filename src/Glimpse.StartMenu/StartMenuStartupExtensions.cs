using System.Text.Json;
using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Configuration;
using Glimpse.StartMenu.Components;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.StartMenu;

public static class StartMenuStartupExtensions
{
	public static Task UseStartMenu(this IHost host)
	{
		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();

		if (!configurationService.ContainsKey(StartMenuConstants.ConfigKey))
		{
			configurationService.Upsert(StartMenuConstants.ConfigKey, StartMenuConfiguration.EmptyJson);
		}

		configurationService
			.ObserveChange(StartMenuConstants.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateStartMenuConfigurationJson(c)));

		return Task.CompletedTask;
	}

	public static void AddStartMenu(this IHostApplicationBuilder builder)
	{
		builder.Services.AddTransient<StartMenuLaunchIcon>();
		builder.Services.AddSingleton<StartMenuWindow>();
		builder.Services.AddSingleton<StartMenuSelectors>();
		builder.Services.AddInstance(StartMenuReducers.AllReducers);
		builder.Services.AddSingleton<IEffectsFactory, StartMenuEffects>();
	}
}