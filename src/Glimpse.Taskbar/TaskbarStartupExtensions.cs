using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Configuration;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using Glimpse.UI.Components.Taskbar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Taskbar;

public static class TaskbarStartupExtensions
{
	public static Task UseTaskbar(this IHost host)
	{
		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();

		configurationService.ConfigurationUpdated.WithLatestFrom(store.Select(TaskbarStateSelectors.Root)).Subscribe(t =>
		{
			var (config, s) = t;
			var slots = config.Taskbar.PinnedLaunchers.Select(l => new SlotRef() { PinnedDesktopFileId = l }).ToImmutableList();
			store.Dispatch(new UpdateTaskbarSlotOrderingBulkAction() { Slots = slots });
		});

		return Task.CompletedTask;
	}

	public static void AddTaskbar(this IHostApplicationBuilder builder)
	{
		builder.Services.AddTransient<TaskbarView>();
		builder.Services.AddSingleton<TaskbarService>();
		builder.Services.AddInstance(TaskbarReducers.AllReducers);
		builder.Services.AddSingleton<IEffectsFactory, TaskbarEffects>();
	}
}
