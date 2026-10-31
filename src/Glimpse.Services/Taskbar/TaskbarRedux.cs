using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.System.Collections;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;

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

	private static readonly ISelector<ImmutableList<WindowProperties>> s_windowPropertiesList = SelectorFactory.Create(
		XorgSelectors.Windows,
		windows => windows.ById.Values.ToImmutableList());

	public static readonly ISelector<SlotReferences> CurrentSlots = SelectorFactory.Create(
		DesktopFileSelectors.DesktopFiles,
		s_windowPropertiesList.WithSequenceComparer((x, y) => x.WindowRef.Id == y.WindowRef.Id && x.ClassHintName.Equals(y.ClassHintName, StringComparison.OrdinalIgnoreCase)),
		s_storedSlots,
		PinnedLaunchers,
		(desktopFiles, windows, storedSlots, pinnedLaunchers) =>
		{
			var files = desktopFiles.ById.Values.ToList();
			var result = ReconcileSlots(storedSlots.Refs, pinnedLaunchers);
			var previouslyFoundDesktopFiles = result
				.Select(l => files.FirstOrDefault(f => f.Id == l.PinnedDesktopFileId))
				.Where(f => f != null)
				.ToList();

			foreach (var window in windows.OrderBy(w => w.CreationDate))
			{
				var desktopFile = FindAppDesktopFileByName(previouslyFoundDesktopFiles, window.ClassHintName) ?? FindAppDesktopFileByName(files, window.ClassHintName);
				var matchingSlot = result.FirstOrDefault(s => s.ClassHintName.Equals(window.ClassHintName, StringComparison.OrdinalIgnoreCase))
					?? result.FirstOrDefault(s => s.PinnedDesktopFileId == desktopFile?.Id)
					?? result.FirstOrDefault(s => s.DiscoveredDesktopFileId == desktopFile?.Id);

				if (matchingSlot == null)
				{
					result = result.Add(new SlotRef() { Id = desktopFile == null ? $"class:{window.ClassHintName.ToUpperInvariant()}" : $"desktop:{desktopFile.Id}", ClassHintName = window.ClassHintName, DiscoveredDesktopFileId = desktopFile?.Id ?? "" });
				}
				else
				{
					result = result.Replace(matchingSlot, matchingSlot with
					{
						ClassHintName = string.IsNullOrEmpty(matchingSlot.ClassHintName) ? window.ClassHintName : matchingSlot.ClassHintName,
						DiscoveredDesktopFileId = desktopFile?.Id ?? matchingSlot.DiscoveredDesktopFileId
					});
				}
			}

			foreach (var unpinnedSlot in result.Where(s => string.IsNullOrEmpty(s.PinnedDesktopFileId)).ToList())
			{
				if (windows.All(w => !w.ClassHintName.Equals(unpinnedSlot.ClassHintName, StringComparison.OrdinalIgnoreCase)))
				{
					result = result.Remove(unpinnedSlot);
				}
			}

			return new SlotReferences { Refs = result };
		});

	internal static ImmutableList<SlotRef> ReconcileSlots(ImmutableList<SlotRef> storedSlots, ImmutableList<string> pinnedLaunchers)
	{
		var pins = pinnedLaunchers.Distinct().Select(id =>
		{
			var slot = storedSlots.FirstOrDefault(s => s.DiscoveredDesktopFileId == id)
				?? new SlotRef { Id = $"desktop:{id}", DiscoveredDesktopFileId = id };
			return slot with { PinnedDesktopFileId = id, DiscoveredDesktopFileId = string.IsNullOrEmpty(slot.ClassHintName) ? "" : slot.DiscoveredDesktopFileId };
		}).ToList();
		var result = ImmutableList<SlotRef>.Empty;
		var nextPin = 0;
		foreach (var slot in storedSlots)
		{
			if (pinnedLaunchers.Contains(slot.DiscoveredDesktopFileId))
			{
				if (nextPin < pins.Count) result = result.Add(pins[nextPin++]);
			}
			else result = result.Add(slot);
		}
		return result.AddRange(pins.Skip(nextPin));
	}

	private static DesktopFile FindAppDesktopFileByName(IEnumerable<DesktopFile> desktopFiles, string classHintName)
	{
		return desktopFiles.FirstOrDefault(f => f.Name.Contains(classHintName, StringComparison.OrdinalIgnoreCase))
			?? desktopFiles.FirstOrDefault(f => f.FileName.Equals(classHintName, StringComparison.OrdinalIgnoreCase))
			?? desktopFiles.FirstOrDefault(f => f.StartupWmClass.Contains(classHintName, StringComparison.OrdinalIgnoreCase))
			?? desktopFiles.FirstOrDefault(f => f.Executable.Contains(classHintName, StringComparison.OrdinalIgnoreCase) && f.Executable.Length == f.CommandLine.Length)
			?? desktopFiles.FirstOrDefault(f => f.Executable.Contains(classHintName, StringComparison.OrdinalIgnoreCase));
	}
}

public record UpdateTaskbarConfigurationAction(TaskbarConfiguration Configuration);
public record ToggleTaskbarPinningAction(string DesktopFileId);
public record UpdateTaskbarSlotOrderingAction(ImmutableList<SlotRef> Slots);

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
		pins = pins.Contains(desktopFileId) ? pins.Remove(desktopFileId)
			: pins.Insert(index < 0 ? pins.Count : slots.Take(index).Count(s => pins.Contains(s.DiscoveredDesktopFileId)), desktopFileId);
		return state with { Configuration = state.Configuration with { PinnedLaunchers = pins } };
	}

	private static TaskbarState UpdateSlotOrdering(TaskbarState state, ImmutableList<SlotRef> slots) => state with
	{
		StoredSlots = new SlotReferences() { Refs = slots.Select(slot => slot with
		{
			DiscoveredDesktopFileId = string.IsNullOrEmpty(slot.DiscoveredDesktopFileId) ? slot.PinnedDesktopFileId : slot.DiscoveredDesktopFileId,
			PinnedDesktopFileId = ""
		}).ToImmutableList() },
		Configuration = state.Configuration with
		{
			PinnedLaunchers = slots.Select(slot => slot.PinnedDesktopFileId).Where(id => !string.IsNullOrEmpty(id)).ToImmutableList()
		}
	};
}
