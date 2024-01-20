using System.Collections.Immutable;
using Glimpse.Configuration;
using Glimpse.Freedesktop.DesktopEntries;
using Glimpse.UI.State;

namespace Glimpse.UI.Components.StartMenu;

public class ActionBarViewModel
{
	public string SettingsButtonCommand { get; set; }
	public string UserSettingsCommand { get; set; }
	public string UserIconPath { get; set; }
	public string PowerButtonCommand { get; set; }
}

public class StartMenuAppViewModel
{
	public int Index { get; set; }
	public DesktopFile DesktopFile { get; set; }
	public ImageViewModel Icon { get; set; }
	public bool IsVisible { get; set; }
	public bool IsPinnedToStartMenu { get; set; }
	public bool IsPinnedToTaskbar { get; set; }
	public Dictionary<string, ImageViewModel> ActionIcons { get; set; }
}

public class StartMenuViewModel
{
	public ImmutableList<StartMenuAppViewModel> AllApps { get; set; }
	public string SearchText { get; set; }
	public bool DisableDragAndDrop { get; set; }
	public ActionBarViewModel ActionBarViewModel { get; set; }
	public ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip> Chips { get; set; }
	public ImmutableList<StartMenuLaunchIconContextMenuItem> LaunchIconContextMenu { get; set; }
	public string StartMenuLaunchIconName { get; set; }
}
