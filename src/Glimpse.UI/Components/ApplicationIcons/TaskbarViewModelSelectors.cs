using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Collections;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Services.Taskbar;
using static MentorLake.Redux.Selectors.SelectorFactory;
using IconInfo = Glimpse.Services.IconInfo;

namespace Glimpse.UI.Components.ApplicationIcons;

internal static class TaskbarViewModelSelectors
{
	private static readonly ISelector<ImmutableList<WindowProperties>> s_windowPropertiesList = Create(
		XorgSelectors.Windows,
		windows => windows.ById.Values.ToImmutableList());

	public static readonly ISelector<SlotReferences> CurrentSlots = Create(
		DesktopFileSelectors.DesktopFiles,
		s_windowPropertiesList.WithSequenceComparer((x, y) => x.WindowRef.Id == y.WindowRef.Id && x.ClassHintName.Equals(y.ClassHintName, StringComparison.OrdinalIgnoreCase)),
		TaskbarSelectors.s_storedSlots,
		(desktopFiles, windows, storedSlots) =>
		{
			var result = ImmutableList<SlotRef>.Empty.AddRange(storedSlots.Refs);

			var previouslyFoundDesktopFiles = result
				.Select(l => desktopFiles.ById.GetValueOrDefault(l.PinnedDesktopFileId))
				.Where(f => f != null)
				.ToList();

			foreach (var window in windows.OrderBy(w => w.CreationDate))
			{
				if (result.Any(s => s.ClassHintName.Equals(window.ClassHintName, StringComparison.OrdinalIgnoreCase))) continue;
				var desktopFile = FindAppDesktopFileByName(previouslyFoundDesktopFiles, window.ClassHintName) ?? FindAppDesktopFileByName(desktopFiles.ById.Values, window.ClassHintName);
				var matchingSlot = result.FirstOrDefault(s => s.PinnedDesktopFileId == desktopFile?.Id) ?? result.FirstOrDefault(s => s.DiscoveredDesktopFileId == desktopFile?.Id);

				if (matchingSlot == null)
				{
					result = result.Add(new SlotRef() { ClassHintName = window.ClassHintName, DiscoveredDesktopFileId = desktopFile?.Id ?? "" });
				}
				else if (string.IsNullOrEmpty(matchingSlot.ClassHintName))
				{
					result = result.Replace(matchingSlot, matchingSlot with { ClassHintName = window.ClassHintName });
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

	private static readonly ISelector<ImmutableList<(SlotRef Slot, DesktopFile DesktopFile)>> s_slotToDesktopFile = Create(
		CurrentSlots,
		DesktopFileSelectors.DesktopFiles,
		(slots, desktopFiles) =>
		{
			return slots.Refs.Select(slot => (
					Slot: slot,
					DesktopFile: desktopFiles.ById.GetValueOrDefault(slot.PinnedDesktopFileId)
					?? desktopFiles.ById.GetValueOrDefault(slot.DiscoveredDesktopFileId)
					?? new DesktopFile() { Id = "" }))
				.ToImmutableList();
		},
		(x, y) => CollectionComparer.Sequence(x, y, (i, j) => i.Slot == j.Slot && i.DesktopFile.Id == j.DesktopFile.Id));

	private static readonly ISelector<ImmutableList<(SlotRef Slot, IconInfo IconInfo)>> s_slotToWindowGroupIcons =
		Create(
			CurrentSlots,
			s_windowPropertiesList.WithSequenceComparer((x, y) => x.WindowRef.Id == y.WindowRef.Id && x.Icons == y.Icons),
			(slots, windows) =>
			{
				return windows
					.Select(window => (
						Slot: slots.Refs.First(s => s.ClassHintName.Equals(window.ClassHintName, StringComparison.OrdinalIgnoreCase)),
						IconInfo: new IconInfo() { Key = window.Id.ToString() }))
					.ToImmutableList();
			});

	private static readonly ISelector<ImmutableList<(SlotRef Slot, IconInfo Icon)>> s_slotToIcon = Create(
		CurrentSlots,
		s_slotToDesktopFile,
		s_slotToWindowGroupIcons,
		(slots, desktopFiles, windowGroups) =>
		{
			return slots.Refs
				.Select(slot =>
				{
					var desktopFile = desktopFiles.FirstOrDefault(f => f.Slot == slot).DesktopFile;

					if (string.IsNullOrEmpty(desktopFile.Id))
					{
						var windowIcon = windowGroups.FirstOrDefault(w => w.Slot == slot).IconInfo;
						return (Slot: slot, Icon: windowIcon);
					}

					return (Slot: slot, Icon: IconInfo.FromNameOrPath(desktopFile?.IconName));
				})
				.ToImmutableList();
		},
		(x, y) => CollectionComparer.Sequence(x, y, (i, j) => i.Slot == j.Slot && i.Icon == j.Icon));

	private static readonly ISelector<ImmutableList<(SlotRef Slot, bool CanClose)>> s_slotToCanClose = Create(
		CurrentSlots,
		s_windowPropertiesList.WithSequenceComparer((x, y) => x.WindowRef.Id == y.WindowRef.Id && x.ClassHintName.Equals(y.ClassHintName, StringComparison.OrdinalIgnoreCase)),
		(slots, windows) =>
		{
			return slots.Refs.Select(slot => (Slot: slot, CanClose: windows.Count(w => w.ClassHintName.Equals(slot.ClassHintName, StringComparison.OrdinalIgnoreCase)) > 0)).ToImmutableList();
		},
		(x, y) => CollectionComparer.Sequence(x, y, (i, j) => i.Slot == j.Slot && i.CanClose == j.CanClose));

	private static readonly ISelector<ImmutableList<(SlotRef Slot, ImmutableList<WindowProperties> Windows)>> s_slotToWindows = Create(
		CurrentSlots,
		s_windowPropertiesList.WithSequenceComparer((x, y) =>
			x.WindowRef.Id == y.WindowRef.Id
			&& x.ClassHintName.Equals(y.ClassHintName, StringComparison.OrdinalIgnoreCase)
			&& x.Geometry == y.Geometry),
		(slots, windows) =>
		{
			return slots.Refs
				.Select(slot => (
					Slot: slot,
					Windows: windows
						.Where(w => w.ClassHintName.Equals(slot.ClassHintName, StringComparison.OrdinalIgnoreCase))
						.ToImmutableList()))
				.ToImmutableList();
		},
		(x, y) => CollectionComparer.Sequence(x, y, (i, j) =>
			i.Slot == j.Slot
			&& CollectionComparer.Sequence(i.Windows, j.Windows, (a, b) => a.WindowRef.Id == b.WindowRef.Id && a.Geometry == b.Geometry)));

	private static readonly ISelector<ImmutableList<(SlotRef Slot, ImmutableList<SlotContextMenuItemViewModel> ViewModel)>> s_contextMenu = Create(
		CurrentSlots,
		s_slotToCanClose,
		s_slotToDesktopFile,
		XorgSelectors.Monitors,
		s_slotToWindows,
		(slots, canClose, desktopFiles, monitors, slotWindows) =>
		{
			return slots.Refs.Select(slot =>
			{
				var desktopFile = desktopFiles.First(f => f.Slot == slot).DesktopFile;
				var isPinned = !string.IsNullOrEmpty(slot.PinnedDesktopFileId);
				var closeable = canClose.First(w => w.Slot == slot).CanClose;
				var contextMenuItems = new List<SlotContextMenuItemViewModel>();

				if (desktopFile.Actions.Any())
				{
					contextMenuItems.Add(new SlotContextMenuItemViewModel() { DisplayText = "Tasks", Id = "header", });
					contextMenuItems.AddRange(desktopFile.Actions.Select(a => new SlotContextMenuItemViewModel()
					{
						Id = a.DesktopFilePath,
						DisplayText = a.ActionName,
						Icon = new ImageViewModel() { IconNameOrPath = desktopFile.IconName },
						DesktopAction = a
					}));
					contextMenuItems.Add(new SlotContextMenuItemViewModel() { DisplayText = "separator" });
				}

				if (!string.IsNullOrEmpty(desktopFile.FilePath))
				{
					contextMenuItems.Add(new SlotContextMenuItemViewModel()
					{
						Id = "Launch",
						DisplayText = desktopFile.Name,
						Icon = new ImageViewModel() { IconNameOrPath = desktopFile.IconName },
						DesktopFilePath = desktopFile.FilePath
					});
				}

				if (!string.IsNullOrEmpty(slot.DiscoveredDesktopFileId) || !string.IsNullOrEmpty(slot.PinnedDesktopFileId))
				{
					contextMenuItems.Add(new SlotContextMenuItemViewModel()
					{
						Id = "Pin",
						DisplayText = isPinned ? "Unpin from taskbar" : "Pin to taskbar",
						Icon = new ImageViewModel() { IconNameOrPath = isPinned ? "list-remove-symbolic" : "list-add-symbolic" },
						DesktopFilePath = desktopFile.FilePath
					});
				}

				if (closeable && monitors.Count > 1)
				{
					var windows = slotWindows.First(w => w.Slot == slot).Windows;
					contextMenuItems.Add(new SlotContextMenuItemViewModel()
					{
						Id = "MoveToMonitor",
						DisplayText = "Move to monitor",
						Icon = new ImageViewModel() { IconNameOrPath = "video-display-symbolic" },
						Children = monitors.Select(m =>
						{
							var isCurrent = windows.Count > 0 && windows.All(w => IsWindowOnMonitor(w, m));
							return new SlotContextMenuItemViewModel()
							{
								Id = "MoveToMonitor",
								DisplayText = m.Name,
								MonitorGeometry = m.WorkArea.IsEmpty ? m.Geometry : m.WorkArea,
								IsChecked = isCurrent,
								IsEnabled = !isCurrent
							};
						}).ToImmutableList()
					});
				}

				if (closeable)
				{
					contextMenuItems.Add(new SlotContextMenuItemViewModel()
					{
						Id = "Close",
						DisplayText = "Close Window",
						Icon = new ImageViewModel() { IconNameOrPath = "window-close-symbolic" },
						DesktopFilePath = desktopFile.FilePath
					});
				}

				return (slot, contextMenuItems.ToImmutableList());
			}).ToImmutableList();
		},
		(x, y) => CollectionComparer.Sequence(x, y));

	private static bool IsWindowOnMonitor(WindowProperties window, MonitorInfo monitor)
	{
		var geometry = window.Geometry;
		if (geometry.IsEmpty) return false;

		var centerX = geometry.X + geometry.Width / 2;
		var centerY = geometry.Y + geometry.Height / 2;
		return monitor.Geometry.Contains(centerX, centerY);
	}
	private static readonly ISelector<ImmutableList<KeyValuePair<IWindowRef, ImageViewModel>>> s_windowToScreenshot = Create(
		CurrentSlots,
		XorgSelectors.Screenshots,
		s_slotToDesktopFile,
		s_windowPropertiesList.WithSequenceComparer((x, y) => x.WindowRef == y.WindowRef && x.Icons == y.Icons),
		(slots, screenshots, desktopFiles, windows) =>
		{
			return windows
				.Select(w =>
				{
					var slot = slots.Refs.FirstOrDefault(s => s.ClassHintName.Equals(w.ClassHintName, StringComparison.OrdinalIgnoreCase));
					var desktopFile = desktopFiles.First(f => f.Slot == slot).DesktopFile;
					var lastScreenshot = screenshots.ById.GetValueOrDefault(w.Id);

					return KeyValuePair.Create(w.WindowRef, new ImageViewModel()
					{
						Image = lastScreenshot ?? w.Icons.MaxBy(i => i.GetWidth()),
						IconNameOrPath = desktopFile.IconName
					});
				})
				.ToImmutableList();
		});

	private static readonly ISelector<ImmutableList<(string ClassHintName, ImmutableList<WindowViewModel> ViewModels)>> s_windowViewModels = Create(
		s_windowToScreenshot,
		s_windowPropertiesList.WithSequenceComparer((x, y) => x.Title == y.Title && x.DemandsAttention == y.DemandsAttention && x.AllowActions == y.AllowActions && x.WindowRef == y.WindowRef && x.Icons == y.Icons),
		(screenshots, windows) => windows
			.GroupBy(w => w.ClassHintName)
			.Select(g =>
			{
				return (ClassHintName: g.Key, g
					.Select(w => new WindowViewModel
					{
						Icon = w.Icons.Any() ? new() { Key =  w.WindowRef.Id.ToString() } : IconInfo.FromNameOrPath(w.IconName),
						Title = w.Title,
						WindowRef = w.WindowRef,
						AllowedActions = w.AllowActions,
						DemandsAttention = w.DemandsAttention,
						Screenshot = screenshots.First(s => s.Key == w.WindowRef).Value
					})
					.ToImmutableList());
			})
			.ToImmutableList());

	internal static readonly ISelector<TaskbarViewModel> ViewModel = Create(
		CurrentSlots,
		s_slotToDesktopFile,
		s_contextMenu,
		s_slotToIcon,
		s_windowViewModels,
		(slots, desktopFiles, contextMenuViewModels, slotsToIcon, windowViewModels) => new TaskbarViewModel
		{
			Groups = slots.Refs
				.Select(slot =>
				{
					var windowViewModelsForSlot = windowViewModels.FirstOrDefault(w => w.ClassHintName.Equals(slot.ClassHintName, StringComparison.OrdinalIgnoreCase)).ViewModels ?? ImmutableList<WindowViewModel>.Empty;

					return new SlotViewModel
					{
						SlotRef = slot,
						DesktopFile = desktopFiles.First(s => s.Slot == slot).DesktopFile,
						Icon = slotsToIcon.First(s => s.Slot == slot).Icon,
						Tasks = windowViewModelsForSlot,
						DemandsAttention = windowViewModelsForSlot.Any(w => w.DemandsAttention),
						ContextMenuItems = contextMenuViewModels.First(vm => vm.Slot == slot).ViewModel
					};
				})
				.ToImmutableList()
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
