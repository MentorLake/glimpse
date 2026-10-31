using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.System.Collections;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;

namespace Glimpse.Services.Taskbar;

public static class TaskbarSelectors
{
	private static readonly ISelector<TaskbarState> s_taskbarState = CreateFeature<TaskbarState>();
	public static readonly ISelector<SlotReferences> s_storedSlots = Create(s_taskbarState, s => s.StoredSlots);
	public static readonly ISelector<TaskbarConfiguration> s_configuration = Create(s_taskbarState, s => s.Configuration);
	public static readonly ISelector<ImmutableList<ContextMenuItem>> s_contextMenu = Create(s_configuration, s => s.ContextMenu);
	public static readonly ISelector<string> s_startMenuLaunchIconName = Create(s_configuration, s => s.StartMenuLaunchIconName);
	public static readonly ISelector<string> TaskManagerCommand = Create(s_configuration, s => s.TaskManagerCommand);
	public static readonly ISelector<ImmutableList<string>> PinnedLaunchers = Create(
		s_configuration,
		s => s.PinnedLaunchers,
		(x, y) => CollectionComparer.Sequence(x, y));

	private static readonly ISelector<ImmutableList<WindowProperties>> s_windowPropertiesList = Create(
		XorgSelectors.Windows,
		windows => windows.ById.Values.ToImmutableList());

	public static readonly ISelector<SlotReferences> CurrentSlots = Create(
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

	private static ImmutableList<SlotRef> ReconcileSlots(ImmutableList<SlotRef> storedSlots, ImmutableList<string> pinnedLaunchers)
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
