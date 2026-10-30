using Glimpse.Libraries.System.Collections;
using MentorLake.Redux.Thunks;

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

public record UpdateTaskbarSlotOrderingBulkAction(ImmutableList<string> SlotIds);
public record UpdateTaskbarConfigurationAction(TaskbarConfiguration Config);
public record UpdateTaskbarSlotsAction(ImmutableList<SlotRef> Slots);

internal static class TaskbarReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new TaskbarState())
			.On<UpdateTaskbarConfigurationAction>((taskbarState, a) =>
			{
				var before = taskbarState.StoredSlots.Refs.Select(i => string.IsNullOrEmpty(i.PinnedDesktopFileId) ? i.DiscoveredDesktopFileId : i.PinnedDesktopFileId).ToList();
				var after = a.Config.PinnedLaunchers.Where(id => !string.IsNullOrEmpty(id)).ToList();
				if (before.SequenceEqual(after))
				{
					return taskbarState with { Configuration = a.Config with { PinnedLaunchers = [.. after] } };
				}
				var results = ImmutableList<SlotRef>.Empty;

				foreach (var pairing in Pairings(before, after))
				{
					if (pairing is LauncherDiff.Removed removed)
					{
						var pinnedSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == removed.DesktopFileId);
						var transientSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == removed.DesktopFileId);
						if (pinnedSlotRef == null && transientSlotRef != null && !results.Contains(transientSlotRef)) results = results.Add(transientSlotRef);
						continue;
					}

					var addedId = pairing switch
					{
						LauncherDiff.Added added => added.DesktopFileId,
						LauncherDiff.Matched matched => matched.NewId,
						_ => throw new InvalidOperationException($"Unexpected pairing {pairing}")
					};

					var slotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == addedId || r.DiscoveredDesktopFileId == addedId)
						?? new SlotRef() { PinnedDesktopFileId = addedId };

					if (!results.Contains(slotRef)) results = results.Add(slotRef);

					if (pairing is LauncherDiff.Matched matchedPairing)
					{
						var pinnedSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.PinnedDesktopFileId == matchedPairing.OldId);
						var transientSlotRef = taskbarState.StoredSlots.Refs.FirstOrDefault(r => r.DiscoveredDesktopFileId == matchedPairing.OldId);
						if (pinnedSlotRef == null && transientSlotRef != null && !results.Contains(transientSlotRef)) results = results.Add(transientSlotRef);
					}
				}

				return taskbarState with { Configuration = a.Config, StoredSlots = new SlotReferences() { Refs = results }};
			})
			.On<UpdateTaskbarSlotsAction>((s, a) => s with { StoredSlots = new SlotReferences() { Refs = a.Slots } })
			.On<UpdateTaskbarSlotOrderingBulkAction>((s, a) =>
			{
				var reordered = a.SlotIds
					.Where(id => !string.IsNullOrEmpty(id))
					.Select(id => s.StoredSlots.Refs.FirstOrDefault(r => r.Id == id))
					.Where(r => r != null)
					.ToImmutableList();
				var remaining = s.StoredSlots.Refs.Where(r => !a.SlotIds.Contains(r.Id));
				return s with { StoredSlots = new SlotReferences() { Refs = reordered.AddRange(remaining) } };
			})
	];

	private abstract record LauncherDiff
	{
		public record Matched(string OldId, string NewId) : LauncherDiff;
		public record Removed(string DesktopFileId) : LauncherDiff;
		public record Added(string DesktopFileId) : LauncherDiff;
	}

	private static List<LauncherDiff> Pairings(List<string> s1, List<string> s2)
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
		List<LauncherDiff> pairs = new List<LauncherDiff>();
		int x = m, y = n;
		while (x > 0 || y > 0)
		{
			if (x > 0 && y > 0 && dp[x, y] == dp[x - 1, y - 1] + (s1[x - 1] == s2[y - 1] ? 0 : 1))
			{
				pairs.Add(new LauncherDiff.Matched(s1[x - 1], s2[y - 1]));
				x--; y--;
			}
			else if (x > 0 && dp[x, y] == dp[x - 1, y] + 1)
			{
				pairs.Add(new LauncherDiff.Removed(s1[x - 1]));
				x--;
			}
			else
			{
				pairs.Add(new LauncherDiff.Added(s2[y - 1]));
				y--;
			}
		}
		pairs.Reverse();
		return pairs;
	}
}
