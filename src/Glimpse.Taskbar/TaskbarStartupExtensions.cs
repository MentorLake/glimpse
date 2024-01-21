using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Configuration;
using MentorLake.Redux;
using Glimpse.UI.Components.Taskbar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Taskbar;

[JsonSerializable(typeof(TaskbarConfiguration))]
internal partial class TaskbarSerializationContext : JsonSerializerContext
{
	public static TaskbarSerializationContext Instance { get; } = new (
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}

public static class TaskbarConstants
{
	public const string ConfigKey = "Taskbar";
}

public static class TaskbarStartupExtensions
{
	public static Task UseTaskbar(this IHost host)
	{
		var store = host.Services.GetRequiredService<ReduxStore>();
		var configurationService = host.Services.GetRequiredService<ConfigurationService>();

		if (!configurationService.ContainsKey(TaskbarConstants.ConfigKey))
		{
			configurationService.Upsert(TaskbarConstants.ConfigKey, new TaskbarConfiguration(), TaskbarSerializationContext.Instance);
		}

		configurationService
			.ObserveChange<TaskbarConfiguration>(TaskbarConstants.ConfigKey, TaskbarSerializationContext.Instance)
			.Subscribe(c =>
			{
				var slots = c.PinnedLaunchers.Select(l => new SlotRef() { PinnedDesktopFileId = l }).ToImmutableList();
				store.Dispatch(new UpdateTaskbarSlotOrderingBulkAction() { Slots = slots });
			});

		store.Select(TaskbarStateSelectors.s_configuration).Subscribe(c =>
		{
			configurationService.Upsert(TaskbarConstants.ConfigKey, c, TaskbarSerializationContext.Instance);
		});

		return Task.CompletedTask;
	}

	public static void AddTaskbar(this IHostApplicationBuilder builder)
	{
		builder.Services.AddTransient<TaskbarView>();
		builder.Services.AddSingleton<TaskbarService>();
		builder.Services.AddInstance(TaskbarReducers.AllReducers);
	}
}
