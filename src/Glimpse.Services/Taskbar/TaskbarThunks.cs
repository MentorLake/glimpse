using Glimpse.Libraries.System.Collections;

namespace Glimpse.Services.Taskbar;

public static class TaskbarThunks
{
	public static Task UpdateConfiguration(TaskbarThunkApi api, TaskbarConfiguration configuration)
	{
		var state = api.State.GetFeatureState<TaskbarState>();
		var before = state.StoredSlots.Refs
			.Select(slot => slot.PinnedDesktopFileId)
			.Where(id => !string.IsNullOrEmpty(id))
			.ToList();
		var after = configuration.PinnedLaunchers.Where(id => !string.IsNullOrEmpty(id)).ToList();
		var slots = state.StoredSlots;

		if (!before.SequenceEqual(after))
		{
			var results = ImmutableList<SlotRef>.Empty;
			foreach (var pairing in SequenceDiff.Compute(before, after))
			{
				if (pairing is SequenceDiff.Removed<string> removed)
				{
					var pinnedSlotRef = state.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == removed.Value);
					var transientSlotRef = state.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == removed.Value);
					if (pinnedSlotRef == null && transientSlotRef != null && !results.Contains(transientSlotRef)) results = results.Add(transientSlotRef);
					continue;
				}

				var addedId = pairing switch
				{
					SequenceDiff.Added<string> added => added.Value,
					SequenceDiff.Matched<string> matched => matched.New,
					_ => throw new InvalidOperationException($"Unexpected pairing {pairing}")
				};

				var slotRef = state.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == addedId || r.DiscoveredDesktopFileId == addedId)
					?? new SlotRef() { PinnedDesktopFileId = addedId };
				if (!results.Contains(slotRef)) results = results.Add(slotRef);

				if (pairing is SequenceDiff.Matched<string> matchedPairing)
				{
					var pinnedSlotRef = state.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == matchedPairing.Old);
					var transientSlotRef = state.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == matchedPairing.Old);
					if (pinnedSlotRef == null && transientSlotRef != null && !results.Contains(transientSlotRef)) results = results.Add(transientSlotRef);
				}
			}

			slots = new SlotReferences() { Refs = results };
		}

		api.Dispatch(new UpdateTaskbarStateAction(state with
		{
			Configuration = configuration with { PinnedLaunchers = [.. after] },
			StoredSlots = slots
		}));

		return Task.CompletedTask;
	}

	public static Task TogglePinning(TaskbarThunkApi api, string desktopFileId)
	{
		var slots = TaskbarSelectors.CurrentSlots.Apply(api.State).Refs;

		if (slots.FirstOrDefault(r => r.PinnedDesktopFileId == desktopFileId) is { } pinnedSlot)
		{
			slots = string.IsNullOrEmpty(pinnedSlot.DiscoveredDesktopFileId)
				? slots.Remove(pinnedSlot)
				: slots.Replace(pinnedSlot, pinnedSlot with { PinnedDesktopFileId = "" });
		}
		else if (slots.FirstOrDefault(r => r.DiscoveredDesktopFileId == desktopFileId) is { } unpinnedSlot)
		{
			slots = slots.Replace(unpinnedSlot, unpinnedSlot with { PinnedDesktopFileId = desktopFileId });
		}
		else
		{
			slots = slots.Add(new SlotRef { PinnedDesktopFileId = desktopFileId });
		}

		return UpdateSlotOrdering(api, slots);
	}

	public static Task UpdateSlotOrdering(TaskbarThunkApi api, ImmutableList<SlotRef> slots)
	{
		var state = api.State.GetFeatureState<TaskbarState>();
		var updated = state with
		{
			StoredSlots = new SlotReferences() { Refs = slots },
			Configuration = state.Configuration with
			{
				PinnedLaunchers = slots.Select(slot => slot.PinnedDesktopFileId)
					.Where(id => !string.IsNullOrEmpty(id))
					.ToImmutableList()
			}
		};
		api.Dispatch(new UpdateTaskbarStateAction(updated));
		api.ConfigurationService.Upsert(TaskbarConfiguration.ConfigKey, updated.Configuration.ToJsonElement());
		return Task.CompletedTask;
	}
}
