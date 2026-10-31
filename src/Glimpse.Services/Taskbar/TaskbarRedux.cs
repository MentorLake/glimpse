using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.System.Collections;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;
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

	private static readonly ISelector<ImmutableList<WindowProperties>> s_windowPropertiesList = SelectorFactory.Create(
		XorgSelectors.Windows,
		windows => windows.ById.Values.ToImmutableList());

	public static readonly ISelector<SlotReferences> CurrentSlots = SelectorFactory.Create(
		DesktopFileSelectors.DesktopFiles,
		s_windowPropertiesList.WithSequenceComparer((x, y) => x.WindowRef.Id == y.WindowRef.Id && x.ClassHintName.Equals(y.ClassHintName, StringComparison.OrdinalIgnoreCase)),
		s_storedSlots,
		(desktopFiles, windows, storedSlots) =>
		{
			var files = desktopFiles.ById.Values.ToList();
			var result = storedSlots.Refs;
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
					result = result.Add(new SlotRef() { ClassHintName = window.ClassHintName, DiscoveredDesktopFileId = desktopFile?.Id ?? "" });
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

	private static DesktopFile FindAppDesktopFileByName(IEnumerable<DesktopFile> desktopFiles, string classHintName)
	{
		return desktopFiles.FirstOrDefault(f => f.Name.Contains(classHintName, StringComparison.OrdinalIgnoreCase))
			?? desktopFiles.FirstOrDefault(f => f.FileName.Equals(classHintName, StringComparison.OrdinalIgnoreCase))
			?? desktopFiles.FirstOrDefault(f => f.StartupWmClass.Contains(classHintName, StringComparison.OrdinalIgnoreCase))
			?? desktopFiles.FirstOrDefault(f => f.Executable.Contains(classHintName, StringComparison.OrdinalIgnoreCase) && f.Executable.Length == f.CommandLine.Length)
			?? desktopFiles.FirstOrDefault(f => f.Executable.Contains(classHintName, StringComparison.OrdinalIgnoreCase));
	}
}

internal record UpdateTaskbarStateAction(TaskbarState State);

internal static class TaskbarReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new TaskbarState())
			.On<UpdateTaskbarStateAction>((_, a) => a.State)
	];
}
