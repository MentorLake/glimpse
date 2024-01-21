using System.Collections.Immutable;
using Glimpse.Common.System;
using Glimpse.Configuration;
using Glimpse.Freedesktop;
using Glimpse.Freedesktop.DesktopEntries;
using Glimpse.StartMenu;
using MentorLake.Redux.Selectors;
using Glimpse.UI.State;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.UI.Components.StartMenu;

public class StartMenuSelectors
{
	public ISelector<StartMenuViewModel> ViewModel { get; }

	public StartMenuSelectors(IStartMenuDemands startMenuDemands)
	{
		var searchTextSelector = Create(UISelectors.StartMenuState, s => s.SearchText);
		var powerButtonCommandSelector = Create(ConfigurationSelectors.Configuration, s => s.PowerButtonCommand);
		var settingsButtonCommandSelector = Create(ConfigurationSelectors.Configuration, s => s.SettingsButtonCommand);
		var taskManagerCommandSelector = Create(ConfigurationSelectors.Configuration, s => s.TaskManagerCommand);
		var userSettingsCommandSelector = Create(ConfigurationSelectors.Configuration, s => s.UserSettingsCommand);
		var chipsSelector = Create(UISelectors.StartMenuState, s => s.Chips);

		var actionBarViewModelSelector = Create(
			powerButtonCommandSelector,
			settingsButtonCommandSelector,
			userSettingsCommandSelector,
			AccountSelectors.UserIconPath,
			(powerButtonCommand, settingsButtonCommand, userSettingsCommand, userIconPath) => new ActionBarViewModel()
			{
				PowerButtonCommand = powerButtonCommand,
				SettingsButtonCommand = settingsButtonCommand,
				UserSettingsCommand = userSettingsCommand,
				UserIconPath = userIconPath
			});

		var allAppsSelector = Create(
			DesktopFileSelectors.AllDesktopFiles,
			searchTextSelector,
			ConfigurationSelectors.Configuration,
			chipsSelector,
			startMenuDemands.TaskbarPinnedLaunchers,
			(allDesktopFiles, searchText, configuration, chips, taskbarPinnedLaunchers) =>
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
					var taskbarIndex = taskbarPinnedLaunchers.IndexOf(f.FilePath);
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

		var menuItemsSelector = Create(
			ConfigurationSelectors.StartMenuLaunchIconContextMenuItems,
			powerButtonCommandSelector,
			settingsButtonCommandSelector,
			taskManagerCommandSelector,
			(menuItems, powerButtonCommand, allSettingsCommands, taskManagerCommand) => menuItems
				.Add(new() { DisplayText = "separator" })
				.Add(new() { DisplayText = "Glimpse config", Executable = "xdg-open", Arguments = ConfigurationFile.FilePath })
				.Add(new() { DisplayText = "Settings", Executable = allSettingsCommands })
				.Add(new() { DisplayText = "Task Manager", Executable = taskManagerCommand })
				.Add(new() { DisplayText = "separator" })
				.Add(new() { DisplayText = "Shutdown or sign out", Executable = powerButtonCommand }));

		ViewModel = Create(
			allAppsSelector,
			searchTextSelector,
			actionBarViewModelSelector,
			chipsSelector,
			menuItemsSelector,
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
					StartMenuLaunchIconName = configuration.StartMenu.StartMenuLaunchIconName
				};
			});
	}


}
