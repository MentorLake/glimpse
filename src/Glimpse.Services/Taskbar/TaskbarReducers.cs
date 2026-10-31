namespace Glimpse.Services.Taskbar;

internal static class TaskbarReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new TaskbarState())
			.On<UpdateTaskbarConfigurationAction>((s, a) => UpdateConfiguration(s, a.Configuration))
			.On<ToggleTaskbarPinningAction>((s, a) => TogglePinning(s, a.DesktopFileId))
			.On<UpdateTaskbarSlotOrderingAction>((s, a) => UpdateSlotOrdering(s, a.Slots))
	];

	private static TaskbarState UpdateConfiguration(TaskbarState state, TaskbarConfiguration configuration)
	{
		return state with { Configuration = configuration with { PinnedLaunchers = configuration.PinnedLaunchers.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToImmutableList() } };
	}

	private static TaskbarState TogglePinning(TaskbarState state, string desktopFileId)
	{
		if (string.IsNullOrEmpty(desktopFileId)) return state;

		var pins = state.Configuration.PinnedLaunchers;
		var slots = state.StoredSlots.Refs;
		var index = slots.FindIndex(s => s.DiscoveredDesktopFileId == desktopFileId);

		pins = pins.Contains(desktopFileId)
			? pins.Remove(desktopFileId)
			: pins.Insert(index < 0 ? pins.Count : slots.Take(index).Count(s => pins.Contains(s.DiscoveredDesktopFileId)), desktopFileId);

		return state with { Configuration = state.Configuration with { PinnedLaunchers = pins } };
	}

	private static TaskbarState UpdateSlotOrdering(TaskbarState state, ImmutableList<SlotRef> slots) => state with
	{
		StoredSlots = new()
		{
			Refs = slots
				.Select(slot => slot with
				{
					DiscoveredDesktopFileId = string.IsNullOrEmpty(slot.DiscoveredDesktopFileId) ? slot.PinnedDesktopFileId : slot.DiscoveredDesktopFileId,
					PinnedDesktopFileId = ""
				})
				.ToImmutableList()
		},
		Configuration = state.Configuration with
		{
			PinnedLaunchers = slots.Select(slot => slot.PinnedDesktopFileId).Where(id => !string.IsNullOrEmpty(id)).ToImmutableList()
		}
	};
}
