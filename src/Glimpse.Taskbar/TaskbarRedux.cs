using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Common.Configuration;
using Glimpse.Common.System.Collections;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar;

internal record SlotReferences
{
	public ImmutableList<SlotRef> Refs = ImmutableList<SlotRef>.Empty;

	public virtual bool Equals(SlotReferences other)
	{
		return other.Refs.SequenceEqual(Refs);
	}
}

internal record SlotRef
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public string PinnedDesktopFileId { get; init; } = "";
	public string ClassHintName { get; init; } = "";
	public string DiscoveredDesktopFileId { get; init; } = "";
}

internal record TaskbarState
{
	public SlotReferences StoredSlots = new();
	public TaskbarConfiguration Configuration = new();

	public virtual bool Equals(TaskbarState other) => ReferenceEquals(this, other);
}

internal static class TaskbarSelectors
{
	private static readonly ISelector<TaskbarState> s_taskbarState = SelectorFactory.CreateFeature<TaskbarState>();
	internal static readonly ISelector<SlotReferences> s_storedSlots = SelectorFactory.Create(s_taskbarState, s => s.StoredSlots);
	internal static readonly ISelector<TaskbarConfiguration> s_configuration = SelectorFactory.Create(s_taskbarState, s => s.Configuration);
	internal static readonly ISelector<ImmutableList<ContextMenuItem>> s_contextMenu = SelectorFactory.Create(s_configuration, s => s.ContextMenu);
	internal static readonly ISelector<string> s_startMenuLaunchIconName = SelectorFactory.Create(s_configuration, s => s.StartMenuLaunchIconName);
	public static readonly ISelector<string> TaskManagerCommand = SelectorFactory.Create(s_configuration, s => s.TaskManagerCommand);
	public static readonly ISelector<ImmutableList<string>> PinnedLaunchers = SelectorFactory.Create(
		s_configuration,
		s => s.PinnedLaunchers,
		(x, y) => CollectionComparer.Sequence(x, y));
}

internal record UpdateTaskbarSlotOrderingBulkAction(ImmutableList<SlotRef> Slots);
internal record UpdateTaskbarConfigurationAction(TaskbarConfiguration Config);
internal record ToggleTaskbarPinningAction(string DesktopFileId);

internal static class TaskbarReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new TaskbarState())
			.On<UpdateTaskbarConfigurationAction>((taskbarState, a) =>
			{
				if (taskbarState.StoredSlots.Refs.Select(s => s.PinnedDesktopFileId).SequenceEqual(a.Config.PinnedLaunchers))
				{
					return taskbarState with { Configuration = a.Config };
				}

				var lastMatchedPinnedLauncherIndex = -1;
				var results = taskbarState.StoredSlots.Refs;

				foreach (var configPinnedDesktopFile in a.Config.PinnedLaunchers)
				{
					var matchIndex = results.FindIndex(s => s.PinnedDesktopFileId == configPinnedDesktopFile);
					if (matchIndex == -1) matchIndex = results.FindIndex(s => s.DiscoveredDesktopFileId == configPinnedDesktopFile);

					if (matchIndex != -1)
					{
						lastMatchedPinnedLauncherIndex = matchIndex;
					}
					else if (lastMatchedPinnedLauncherIndex == -1)
					{
						results = results.Insert(0, new SlotRef() { PinnedDesktopFileId = configPinnedDesktopFile });
						lastMatchedPinnedLauncherIndex = 0;
					}
					else
					{
						results = results.Insert(matchIndex + 1, new SlotRef() { PinnedDesktopFileId = configPinnedDesktopFile });
						lastMatchedPinnedLauncherIndex = matchIndex;
					}
				}

				return taskbarState with { Configuration = a.Config, StoredSlots = new SlotReferences() { Refs = results }};
			})
			.On<ToggleTaskbarPinningAction>((s, a) =>
			{
				if (s.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == a.DesktopFileId) is { } pinnedSlot)
				{
					return string.IsNullOrEmpty(pinnedSlot.DiscoveredDesktopFileId)
						? s with { StoredSlots = s.StoredSlots with { Refs = s.StoredSlots.Refs.Remove(pinnedSlot) } }
						: s with { StoredSlots = s.StoredSlots with { Refs = s.StoredSlots.Refs.Replace(pinnedSlot, pinnedSlot with { PinnedDesktopFileId = "" }) } };
				}

				if (s.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == a.DesktopFileId) is { } unpinnedSlot)
				{
					return s with { StoredSlots = s.StoredSlots with { Refs = s.StoredSlots.Refs.Replace(unpinnedSlot, unpinnedSlot with { PinnedDesktopFileId = a.DesktopFileId }) } };
				}

				return s with { StoredSlots = s.StoredSlots with { Refs = s.StoredSlots.Refs.Add(new SlotRef() { PinnedDesktopFileId = a.DesktopFileId }) } };
			})
			.On<UpdateTaskbarSlotOrderingBulkAction>((s, a) =>
			{
				return s with { StoredSlots = new SlotReferences() { Refs = a.Slots } };
			})
	];
}

internal class TaskbarEffects(ReduxStore store, ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => [
		EffectsFactory.Create(actions => actions
			.OfType<UpdateTaskbarSlotOrderingBulkAction>()
			.WithLatestFrom(store.Select(TaskbarSelectors.s_configuration))
			.WithLatestFrom(store.Select(TaskbarSelectors.s_storedSlots))
			.Do(t =>
			{
				var ((_, config), currentSlots) = t;
				var pinnedLaunchers = currentSlots.Refs.Select(slot => slot.PinnedDesktopFileId).Where(x => !string.IsNullOrEmpty(x)).ToImmutableList();
				var newConfig = config with { PinnedLaunchers = pinnedLaunchers };
				configurationService.Upsert(TaskbarConfiguration.ConfigKey, newConfig.ToJsonElement());
			})),
		EffectsFactory.Create(actions => actions
			.OfType<ToggleTaskbarPinningAction>()
			.WithLatestFrom(store.Select(TaskbarSelectors.s_configuration))
			.WithLatestFrom(store.Select(TaskbarSelectors.s_storedSlots))
			.Do(t =>
			{
				var ((_, config), currentSlots) = t;
				var pinnedLaunchers = currentSlots.Refs.Select(slot => slot.PinnedDesktopFileId).Where(x => !string.IsNullOrEmpty(x)).ToImmutableList();
				var newConfig = config with { PinnedLaunchers = pinnedLaunchers };
				configurationService.Upsert(TaskbarConfiguration.ConfigKey, newConfig.ToJsonElement());
			}))
	];
}
