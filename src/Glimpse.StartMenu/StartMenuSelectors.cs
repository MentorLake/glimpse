using System.Collections.Immutable;
using Glimpse.Common.System;
using Glimpse.Configuration;
using Glimpse.Freedesktop;
using Glimpse.Freedesktop.DesktopEntries;
using MentorLake.Redux.Selectors;
using Glimpse.UI.State;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.UI.Components.StartMenu;

public static class StartMenuSelectors
{
	private static readonly ISelector<string> s_searchText = Create(UISelectors.StartMenuState, s => s.SearchText);
	private static readonly ISelector<string> s_powerButtonCommand = Create(ConfigurationSelectors.Configuration, s => s.PowerButtonCommand);
	private static readonly ISelector<string> s_settingsButtonCommand = Create(ConfigurationSelectors.Configuration, s => s.SettingsButtonCommand);
	private static readonly ISelector<string> s_taskManagerCommand = Create(ConfigurationSelectors.Configuration, s => s.TaskManagerCommand);
	private static readonly ISelector<string> s_userSettingsCommand = Create(ConfigurationSelectors.Configuration, s => s.UserSettingsCommand);
	private static readonly ISelector<ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip>> s_chips = Create(UISelectors.StartMenuState, s => s.Chips);

	private static readonly ISelector<ActionBarViewModel> s_actionBarViewModelSelector = Create(
		s_powerButtonCommand,
		s_settingsButtonCommand,
		s_userSettingsCommand,
		AccountSelectors.UserIconPath,
		(powerButtonCommand, settingsButtonCommand, userSettingsCommand, userIconPath) => new ActionBarViewModel()
		{
			PowerButtonCommand = powerButtonCommand,
			SettingsButtonCommand = settingsButtonCommand,
			UserSettingsCommand = userSettingsCommand,
			UserIconPath = userIconPath
		});

	private static readonly ISelector<ImmutableList<StartMenuAppViewModel>> s_allAppsSelector = Create(
		DesktopFileSelectors.AllDesktopFiles,
		s_searchText,
		ConfigurationSelectors.Configuration,
		s_chips,
		(allDesktopFiles, searchText, configuration, chips) =>
		{
			var results = new LinkedList<StartMenuAppViewModel>();
			var index = 0;
			var isShowingSearchResults = chips[StartMenuChips.SearchResults].IsSelected;
			var isShowingPinned = chips[StartMenuChips.Pinned].IsSelected;
			var isShowingAllApps = chips[StartMenuChips.AllApps].IsSelected;
			var lowerCaseSearchText = searchText.ToLower();

			foreach (var f in allDesktopFiles)
			{
				var pinnedIndex = configuration.StartMenu.PinnedLaunchers.IndexOf(f.FilePath);
				var taskbarIndex = configuration.Taskbar.PinnedLaunchers.IndexOf(f.FilePath);
				var isSearchMatch = isShowingSearchResults && lowerCaseSearchText.AllCharactersIn(f.Name.ToLower());
				var isPinned = pinnedIndex > -1;
				var isVisible = isShowingAllApps || (isShowingSearchResults && isSearchMatch) || (isShowingPinned && isPinned);

				var appViewModel = new StartMenuAppViewModel();
				appViewModel.DesktopFile = f;
				appViewModel.Icon = new ImageViewModel() { IconNameOrPath = f.IconName };
				appViewModel.IsPinnedToTaskbar = taskbarIndex > -1;
				appViewModel.IsPinnedToStartMenu = pinnedIndex > -1;
				appViewModel.IsVisible = isVisible;
				appViewModel.Index = (isShowingSearchResults || isShowingAllApps) && isVisible ? index++ : pinnedIndex;
				appViewModel.ActionIcons = f.Actions.ToDictionary(a => a.ActionName, a => new ImageViewModel() { IconNameOrPath = f.IconName });

				results.AddLast(appViewModel);
			}

			return results.OrderBy(r => r.Index).ToImmutableList();
		});

	private static readonly ISelector<ImmutableList<StartMenuLaunchIconContextMenuItem>> s_menuItems = Create(
		ConfigurationSelectors.StartMenuLaunchIconContextMenuItems,
		s_powerButtonCommand,
		s_settingsButtonCommand,
		s_taskManagerCommand,
		(menuItems, powerButtonCommand, allSettingsCommands, taskManagerCommand) => menuItems
			.Add(new() { DisplayText = "separator" })
			.Add(new() { DisplayText = "Glimpse config", Executable = "xdg-open", Arguments = ConfigurationFile.FilePath })
			.Add(new() { DisplayText = "Settings", Executable = allSettingsCommands })
			.Add(new() { DisplayText = "Task Manager", Executable = taskManagerCommand })
			.Add(new() { DisplayText = "separator" })
			.Add(new() { DisplayText = "Shutdown or sign out", Executable = powerButtonCommand }));

	public static readonly ISelector<StartMenuViewModel> ViewModel = Create(
		s_allAppsSelector,
		s_searchText,
		s_actionBarViewModelSelector,
		s_chips,
		s_menuItems,
		ConfigurationSelectors.Configuration,
		(allApps, searchText, actionBarViewModel, chips, menuItems, configuration) =>
		{
			return new StartMenuViewModel()
			{
				AllApps = allApps,
				SearchText = searchText,
				DisableDragAndDrop = searchText.Length > 0,
				ActionBarViewModel = actionBarViewModel,
				Chips = chips,
				LaunchIconContextMenu = menuItems,
				StartMenuLaunchIconName = configuration.Taskbar.StartMenuLaunchIconName
			};
		});
}
