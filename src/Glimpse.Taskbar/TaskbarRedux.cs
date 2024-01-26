using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Common.System.Collections;
using Glimpse.Common.System.Collections.Immutable;
using Glimpse.Configuration;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar;

public record SlotReferences
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
	public SlotReferences StoredSlots = new();
	public TaskbarConfiguration Configuration = new();

	public virtual bool Equals(TaskbarState other) => ReferenceEquals(this, other);
}

public static class TaskbarSelectors
{
	private static readonly ISelector<TaskbarState> s_taskbarState = SelectorFactory.CreateFeature<TaskbarState>();
	internal static readonly ISelector<SlotReferences> StoredSlots = SelectorFactory.Create(s_taskbarState, s => s.StoredSlots);
	internal static readonly ISelector<TaskbarConfiguration> s_configuration = SelectorFactory.Create(s_taskbarState, s => s.Configuration);
	public static readonly ISelector<ImmutableList<string>> PinnedLaunchers = SelectorFactory.Create(
		s_configuration,
		s => s.PinnedLaunchers,
		(x, y) => CollectionComparer.Sequence(x, y));
}

public record UpdateTaskbarSlotOrderingBulkAction(ImmutableList<SlotRef> Slots);
public record UpdateTaskbarConfigurationAction(TaskbarConfiguration Config);
public record ToggleTaskbarPinningAction(string DesktopFileId);

public static class TaskbarReducers
{
	internal static IList<TR> FullOuterJoin<TA, TB, TK, TR>(
		this IEnumerable<TA> a,
		IEnumerable<TB> b,
		Func<TA, TK> selectKeyA,
		Func<TB, TK> selectKeyB,
		Func<TA, TB, TK, TR> projection,
		TA defaultA = default(TA),
		TB defaultB = default(TB),
		IEqualityComparer<TK> cmp = null)
	{
		cmp = cmp?? EqualityComparer<TK>.Default;
		var alookup = a.ToLookup(selectKeyA, cmp);
		var blookup = b.ToLookup(selectKeyB, cmp);

		var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
		keys.UnionWith(blookup.Select(p => p.Key));

		var join = from key in keys
			from xa in alookup[key].DefaultIfEmpty(defaultA)
			from xb in blookup[key].DefaultIfEmpty(defaultB)
			select projection(xa, xb, key);

		return join.ToList();
	}

	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new TaskbarState())
			.On<UpdateTaskbarConfigurationAction>((taskbarState, a) =>
			{
				if (taskbarState.StoredSlots.Refs.Select(s => s.PinnedDesktopFileId).SequenceEqual(a.Config.PinnedLaunchers))
				{
					return taskbarState with { Configuration = a.Config };
				}

				var join = taskbarState.StoredSlots.Refs.FullOuterJoin(
					a.Config.PinnedLaunchers,
					s => s.PinnedDesktopFileId,
					s => s, (slotRef, pinnedLauncher, _) => (slotRef, pinnedLauncher));

				var results = new List<SlotRef>();

				// This isn't handling all cases.
				foreach (var x in join)
				{
					if (x.slotRef == null)
					{
						results.Add(new SlotRef() { PinnedDesktopFileId = x.pinnedLauncher });
					}
					else if (string.IsNullOrEmpty(x.pinnedLauncher))
					{
						results.Add(x.slotRef);
					}
					else if (!string.IsNullOrEmpty(x.slotRef.PinnedDesktopFileId) && !string.IsNullOrEmpty(x.pinnedLauncher))
					{
						results.Add(x.slotRef);
					}
					else if (string.IsNullOrEmpty(x.slotRef.PinnedDesktopFileId) && string.IsNullOrEmpty(x.pinnedLauncher))
					{
						results.Add(x.slotRef with { PinnedDesktopFileId = x.pinnedLauncher });
					}
				}

				return taskbarState with { Configuration = a.Config, StoredSlots = new SlotReferences() { Refs = results.ToImmutableList() }};
			})
			.On<ToggleTaskbarPinningAction>((s, a) =>
			{
				var slotToToggle = s.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == a.DesktopFileId)
					?? s.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == a.DesktopFileId)
					?? new SlotRef() { PinnedDesktopFileId = a.DesktopFileId };

				return s with { StoredSlots = s.StoredSlots with { Refs = s.StoredSlots.Refs.Toggle(slotToToggle) } };
			})
			.On<UpdateTaskbarSlotOrderingBulkAction>((s, a) =>
			{
				return s with { StoredSlots = new SlotReferences() { Refs = a.Slots } };
			})
	];
}

public class TaskbarEffects(ReduxStore store, ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => [
		EffectsFactory.Create(actions => actions
			.OfType<UpdateTaskbarSlotOrderingBulkAction>()
			.WithLatestFrom(store.Select(TaskbarSelectors.s_configuration))
			.WithLatestFrom(store.Select(TaskbarSelectors.StoredSlots))
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
			.WithLatestFrom(store.Select(TaskbarSelectors.StoredSlots))
			.Do(t =>
			{
				var ((_, config), currentSlots) = t;
				var pinnedLaunchers = currentSlots.Refs.Select(slot => slot.PinnedDesktopFileId).Where(x => !string.IsNullOrEmpty(x)).ToImmutableList();
				var newConfig = config with { PinnedLaunchers = pinnedLaunchers };
				configurationService.Upsert(TaskbarConfiguration.ConfigKey, newConfig.ToJsonElement());
			}))
	];
}
