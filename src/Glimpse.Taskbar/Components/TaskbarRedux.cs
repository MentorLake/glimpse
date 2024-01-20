using System.Collections.Immutable;
using Glimpse.Common.System.Collections.Immutable;
using Glimpse.Configuration;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar;

public class SlotReferences : IEquatable<SlotReferences>
{
	public ImmutableList<SlotRef> Refs = ImmutableList<SlotRef>.Empty;

	public bool Equals(SlotReferences other)
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
}

public class UpdateTaskbarSlotOrderingBulkAction
{
	public ImmutableList<SlotRef> Slots { get; set; }
}

public record ToggleTaskbarPinningAction(string DesktopFileId);

public class TaskbarReducers
{
	public static readonly FeatureReducerCollection AllReducers = new()
	{
		FeatureReducer.Build(new TaskbarState())
			.On<UpdateTaskbarSlotOrderingBulkAction>((s, a) => s with { TaskbarSlots = new SlotReferences() { Refs = a.Slots } })
	};
}

public class TaskbarEffects(ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.CreateEffect<ToggleTaskbarPinningAction, ConfigurationFile>(
			ConfigurationSelectors.Configuration,
			(a, s) =>
			{
				configurationService.UpdateConfiguration(s with { Taskbar = s.Taskbar with { PinnedLaunchers = s.Taskbar.PinnedLaunchers.Toggle(a.DesktopFileId) } });
			}),
		EffectsFactory.CreateEffect<UpdateTaskbarSlotOrderingBulkAction, ConfigurationFile>(
			ConfigurationSelectors.Configuration,
			(a, s) =>
			{
				var pinnedSlots = a.Slots.Select(r => r.PinnedDesktopFileId).Where(slot => !string.IsNullOrEmpty(slot)).ToImmutableList();
				if (pinnedSlots.SequenceEqual(s.Taskbar.PinnedLaunchers)) return;
				configurationService.UpdateConfiguration(s with { Taskbar = s.Taskbar with { PinnedLaunchers = pinnedSlots } });
			})
	};
}
