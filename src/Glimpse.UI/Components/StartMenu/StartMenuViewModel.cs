using System.Collections.Immutable;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ContextMenu;

namespace Glimpse.UI.Components.StartMenu;

public record StartMenuAppContextMenuItem : IContextMenuItemViewModel<StartMenuAppContextMenuItem>
{
	public const string ToggleTaskbarAppId = "ToggleTaskbarAppId";
	public const string ToggleStartMenuAppId = "ToggleStartMenuAppId";

	public DesktopFileAction DesktopAction { get; set; }
	public DesktopFile DesktopFile { get; set; }
	public string DisplayText { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public string Id { get; set; }
	public ImmutableList<StartMenuAppContextMenuItem> Children { get; set; } = ImmutableList<StartMenuAppContextMenuItem>.Empty;
}

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
	public IconInfo Icon { get; set; }
	public bool IsPinnedToStartMenu { get; set; }
	public bool IsPinnedToTaskbar { get; set; }
	public Dictionary<string, ImageViewModel> ActionIcons { get; set; }
	public int PinnedIndex { get; set; }
}

public class StartMenuViewModel
{
	public ImmutableDictionary<string, StartMenuAppViewModel> AllApps { get; set; } = ImmutableDictionary<string, StartMenuAppViewModel>.Empty;
	public ActionBarViewModel ActionBarViewModel { get; set; }
	public ImmutableList<string> SearchResults { get; set; }
}
