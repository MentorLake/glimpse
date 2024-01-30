using System.Collections.Immutable;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;

namespace Glimpse.StartMenu.Components;

internal record StartMenuAppContextMenuItem : IContextMenuItemViewModel<StartMenuAppContextMenuItem>
{
	public const string ToggleTaskbarAppId = "ToggleTaskbarAppId";
	public const string ToggleStartMenuAppId = "ToggleStartMenuAppId";

	public DesktopFileAction DesktopAction { get; set; }
	public string DesktopFilePath { get; set; }
	public string DisplayText { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public string Id { get; set; }
	public ImmutableList<StartMenuAppContextMenuItem> Children { get; set; } = ImmutableList<StartMenuAppContextMenuItem>.Empty;
}

internal class ActionBarViewModel
{
	public string SettingsButtonCommand { get; set; }
	public string UserSettingsCommand { get; set; }
	public string UserIconPath { get; set; }
	public string PowerButtonCommand { get; set; }
}

internal class StartMenuAppViewModel
{
	public int Index { get; set; }
	public DesktopFile DesktopFile { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public bool IsVisible { get; set; }
	public bool IsPinnedToStartMenu { get; set; }
	public bool IsPinnedToTaskbar { get; set; }
	public Dictionary<string, ImageViewModel> ActionIcons { get; set; }
	public ImmutableList<StartMenuAppContextMenuItem> ContextMenuItems { get; set; } = ImmutableList<StartMenuAppContextMenuItem>.Empty;
}

internal class StartMenuViewModel
{
	public ImmutableList<StartMenuAppViewModel> AllApps { get; set; } = ImmutableList<StartMenuAppViewModel>.Empty;
	public string SearchText { get; set; }
	public bool DisableDragAndDrop { get; set; }
	public ActionBarViewModel ActionBarViewModel { get; set; }
	public ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip> Chips { get; set; } = ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip>.Empty;
}
