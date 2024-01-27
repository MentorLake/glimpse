using System.Collections.Immutable;
using Glimpse.Common.Freedesktop.DesktopEntries;
using Glimpse.Common.Gtk;

namespace Glimpse.StartMenu.Components;

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
	public ImageViewModel Icon { get; set; }
	public bool IsVisible { get; set; }
	public bool IsPinnedToStartMenu { get; set; }
	public bool IsPinnedToTaskbar { get; set; }
	public Dictionary<string, ImageViewModel> ActionIcons { get; set; }
}

internal class StartMenuViewModel
{
	public ImmutableList<StartMenuAppViewModel> AllApps { get; set; }
	public string SearchText { get; set; }
	public bool DisableDragAndDrop { get; set; }
	public ActionBarViewModel ActionBarViewModel { get; set; }
	public ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip> Chips { get; set; }
}
