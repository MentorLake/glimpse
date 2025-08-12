using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.System.Collections;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Services.Taskbar;

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

public static class TaskbarSelectors
{
	private static readonly ISelector<TaskbarState> s_taskbarState = SelectorFactory.CreateFeature<TaskbarState>();
	public static readonly ISelector<SlotReferences> s_storedSlots = SelectorFactory.Create(s_taskbarState, s => s.StoredSlots);
	public static readonly ISelector<TaskbarConfiguration> s_configuration = SelectorFactory.Create(s_taskbarState, s => s.Configuration);
	public static readonly ISelector<ImmutableList<ContextMenuItem>> s_contextMenu = SelectorFactory.Create(s_configuration, s => s.ContextMenu);
	public static readonly ISelector<string> s_startMenuLaunchIconName = SelectorFactory.Create(s_configuration, s => s.StartMenuLaunchIconName);
	public static readonly ISelector<string> TaskManagerCommand = SelectorFactory.Create(s_configuration, s => s.TaskManagerCommand);
	public static readonly ISelector<ImmutableList<string>> PinnedLaunchers = SelectorFactory.Create(
		s_configuration,
		s => s.PinnedLaunchers,
		(x, y) => CollectionComparer.Sequence(x, y));
}

public record UpdateTaskbarSlotOrderingBulkAction(ImmutableList<SlotRef> Slots);
public record UpdateTaskbarConfigurationAction(TaskbarConfiguration Config);
public record ToggleTaskbarPinningAction(string DesktopFileId);

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

				var before = taskbarState.StoredSlots.Refs.Select(i => string.IsNullOrEmpty(i.PinnedDesktopFileId) ? i.DiscoveredDesktopFileId : i.PinnedDesktopFileId).ToList();
				var after = a.Config.PinnedLaunchers.ToList();
				var results = ImmutableList<SlotRef>.Empty;

				foreach (var p in Pairings(before, after))
				{
					if (string.IsNullOrEmpty(p.Item1) && !string.IsNullOrEmpty(p.Item2))
					{
						var slotRef = taskbarState.StoredSlots.Refs.Find(r => r.PinnedDesktopFileId == p.Item2);
						if (slotRef == null) slotRef = new SlotRef() { PinnedDesktopFileId = p.Item2 };
						results = results.Add(slotRef);
					}
					else if (!string.IsNullOrEmpty(p.Item1) && string.IsNullOrEmpty(p.Item2))
					{
						var pinnedSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == p.Item1);
						var transientSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == p.Item1);
						if (pinnedSlotRef == null && transientSlotRef != null) results = results.Add(transientSlotRef);
					}
					else
					{
						var slotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == p.Item2 || r.DiscoveredDesktopFileId == p.Item2);
						if (slotRef == null) slotRef = new SlotRef() { PinnedDesktopFileId = p.Item2 };
						results = results.Add(slotRef);

						var pinnedSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == p.Item1);
						var transientSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == p.Item1);
						if (pinnedSlotRef == null && transientSlotRef != null) results = results.Add(transientSlotRef);
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

	private static List<(string, string)> Pairings(List<string> s1, List<string> s2)
	{
		int m = s1.Count, n = s2.Count;
		int[,] dp = new int[m + 1, n + 1];
		for (int i = 0; i <= m; i++) dp[i, 0] = i;
		for (int j = 0; j <= n; j++) dp[0, j] = j;
		for (int i = 1; i <= m; i++)
		{
			for (int j = 1; j <= n; j++)
			{
				int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
				dp[i, j] = Math.Min(Math.Min(dp[i - 1, j - 1] + cost, dp[i - 1, j] + 1), dp[i, j - 1] + 1);
			}
		}
		List<string> aligned1 = new List<string>();
		List<string> aligned2 = new List<string>();
		int x = m, y = n;
		while (x > 0 || y > 0)
		{
			if (x > 0 && y > 0 && dp[x, y] == dp[x - 1, y - 1] + (s1[x - 1] == s2[y - 1] ? 0 : 1))
			{
				aligned1.Add(s1[x - 1]);
				aligned2.Add(s2[y - 1]);
				x--; y--;
			}
			else if (x > 0 && dp[x, y] == dp[x - 1, y] + 1)
			{
				aligned1.Add(s1[x - 1]);
				aligned2.Add(null);
				x--;
			}
			else
			{
				aligned1.Add(null);
				aligned2.Add(s2[y - 1]);
				y--;
			}
		}
		aligned1.Reverse();
		aligned2.Reverse();
		List<(string, string)> pairs = new List<(string, string)>();
		for (int i = 0; i < aligned1.Count; i++)
		{
			pairs.Add((aligned1[i], aligned2[i]));
		}
		return pairs;
	}
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
