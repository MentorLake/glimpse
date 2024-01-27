using Glimpse.Common.Configuration;
using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Taskbar.Components;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Taskbar;

public static class TaskbarStartupExtensions
{
	public static Task UseTaskbar(this IHost host)
	{
		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();
		configurationService.AddIfNotExists(TaskbarConfiguration.ConfigKey, TaskbarConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(TaskbarConfiguration.ConfigKey)
			.Subscribe(c => store.Dispatch(new UpdateTaskbarConfigurationAction(TaskbarConfiguration.From(c))));

		return Task.CompletedTask;
	}

	public static void AddTaskbar(this IHostApplicationBuilder builder)
	{
		builder.Services.AddTransient<TaskbarView>();
		builder.Services.AddSingleton<TaskbarService>();
		builder.Services.AddSingleton<IEffectsFactory, TaskbarEffects>();
		builder.Services.AddInstance(TaskbarReducers.AllReducers);
	}
}
