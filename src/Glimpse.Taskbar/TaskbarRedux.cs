using System.Collections.Immutable;
using Glimpse.Common.System.Collections;
using Glimpse.Common.System.Collections.Immutable;
using Glimpse.Configuration;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar;

public record SlotReferences : IEquatable<SlotReferences>
{
	public ImmutableList<SlotRef> Refs = ImmutableList<SlotRef>.Empty;

	public virtual bool Equals(SlotReferences other)
	{
		return other.Refs.SequenceEqual(Refs);
	}
}

public record SlotRef
{
	public string PinnedDesktopFileId { get; init; } = "";
	public string ClassHintName { get; init; } = "";
	public string DiscoveredDesktopFileId { get; init; } = "";
}

public record TaskbarState
{
	public SlotReferences TaskbarSlots = new();

	public virtual bool Equals(TaskbarState other) => ReferenceEquals(this, other);
}

public class TaskbarStateSelectors
{
	public static readonly ISelector<TaskbarState> Root = SelectorFactory.CreateFeature<TaskbarState>();
	public static readonly ISelector<SlotReferences> UserSortedSlots = SelectorFactory.Create(Root, s => s.TaskbarSlots);

	public static readonly ISelector<ImmutableList<string>> PinnedLaunchers = SelectorFactory.Create(
		UserSortedSlots,
		s => s.Refs.Select(r => r.PinnedDesktopFileId).Where(x => !string.IsNullOrEmpty(x)).ToImmutableList(),
		(x, y) => CollectionComparer.Sequence(x, y, (x1, y1) => x1.SequenceEqual(y1)));

	internal static readonly ISelector<TaskbarConfiguration> s_configuration = SelectorFactory.Create(
		PinnedLaunchers,
		s => new TaskbarConfiguration() { PinnedLaunchers = s });
}

public class UpdateTaskbarSlotOrderingBulkAction
{
	public ImmutableList<SlotRef> Slots { get; set; }
}

public record ToggleTaskbarPinningAction(string DesktopFileId);

public class TaskbarReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new TaskbarState())
			.On<ToggleTaskbarPinningAction>((s, a) =>
			{
				var slotToToggle = s.TaskbarSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == a.DesktopFileId)
					?? s.TaskbarSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == a.DesktopFileId)
					?? new SlotRef() { PinnedDesktopFileId = a.DesktopFileId };

				return s with { TaskbarSlots = s.TaskbarSlots with { Refs = s.TaskbarSlots.Refs.Toggle(slotToToggle) } };
			})
			.On<UpdateTaskbarSlotOrderingBulkAction>((s, a) =>
			{
				return s with { TaskbarSlots = new SlotReferences() { Refs = a.Slots } };
			})
	];
}
