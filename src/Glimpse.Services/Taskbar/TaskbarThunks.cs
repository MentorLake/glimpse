using MentorLake.Redux.Thunks;

namespace Glimpse.Services.Taskbar;

public sealed class TaskbarThunkApi(ThunkApiContext context, ConfigurationService configurationService) : ThunkApi(context)
{
	public ConfigurationService ConfigurationService { get; } = configurationService;
}

public static class TaskbarThunks
{
	private static Task PersistPinnedLaunchers(TaskbarThunkApi api)
	{
		var state = api.State.GetFeatureState<TaskbarState>();
		var pinnedLaunchers = state.StoredSlots.Refs
			.Select(slot => slot.PinnedDesktopFileId)
			.Where(id => !string.IsNullOrEmpty(id))
			.ToImmutableList();
		api.ConfigurationService.Upsert(TaskbarConfiguration.ConfigKey, (state.Configuration with { PinnedLaunchers = pinnedLaunchers }).ToJsonElement());
		return Task.CompletedTask;
	}

	public static async Task TogglePinning(TaskbarThunkApi api, string desktopFileId)
	{
		var slots = api.State.GetFeatureState<TaskbarState>().StoredSlots.Refs;

		if (slots.FirstOrDefault(r => r.PinnedDesktopFileId == desktopFileId) is { } pinnedSlot)
		{
			var updated = string.IsNullOrEmpty(pinnedSlot.DiscoveredDesktopFileId)
				? slots.Remove(pinnedSlot)
				: slots.Replace(pinnedSlot, pinnedSlot with { PinnedDesktopFileId = "" });
			api.Dispatch(new UpdateTaskbarSlotsAction(updated));
		}
		else if (slots.FirstOrDefault(r => r.DiscoveredDesktopFileId == desktopFileId) is { } unpinnedSlot)
		{
			api.Dispatch(new UpdateTaskbarSlotsAction(slots.Replace(unpinnedSlot, unpinnedSlot with { PinnedDesktopFileId = desktopFileId })));
		}
		else
		{
			api.Dispatch(new UpdateTaskbarSlotsAction(slots.Add(new SlotRef { PinnedDesktopFileId = desktopFileId })));
		}

		await api.Using<TaskbarThunkApi>().Dispatch(PersistPinnedLaunchers).ToTask();
	}

	public static async Task UpdateSlotOrdering(TaskbarThunkApi api, ImmutableList<string> slotIds)
	{
		api.Dispatch(new UpdateTaskbarSlotOrderingBulkAction(slotIds));
		await api.Using<TaskbarThunkApi>().Dispatch(PersistPinnedLaunchers).ToTask();
	}
}
