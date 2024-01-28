using System.Collections.Immutable;
using Glimpse.Common.Accounts;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.System;
using Glimpse.StartMenu.Components;
using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.StartMenu;

internal class StartMenuSelectors
{
	private static readonly ISelector<StartMenuState> s_startMenuState = CreateFeature<StartMenuState>();
	internal static readonly ISelector<StartMenuConfiguration> s_configuration = Create(s_startMenuState, s => s.Configuration);
	private static readonly ISelector<ImmutableList<string>> s_pinnedLaunchers = Create(s_configuration, s => s.PinnedLaunchers);
	private static readonly ISelector<string> s_powerButtonCommand = Create(s_configuration, s => s.PowerButtonCommand);
	private static readonly ISelector<string> s_settingsButtonCommand = Create(s_configuration, s => s.SettingsButtonCommand);
	private static readonly ISelector<string> s_userSettingsCommand = Create(s_configuration, s => s.UserSettingsCommand);
	private static readonly ISelector<string> s_searchTextSelector = Create(s_startMenuState, s => s.SearchText);
	private static readonly ISelector<ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip>> s_chipsSelector = Create(s_startMenuState, s => s.Chips);

	public ISelector<StartMenuViewModel> ViewModel { get; }

	public StartMenuSelectors(IStartMenuDemands startMenuDemands)
	{
		var actionBarViewModelSelector = Create(
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

		var allAppsSelector = Create(
			DesktopFileSelectors.AllDesktopFiles,
			s_searchTextSelector,
			s_chipsSelector,
			startMenuDemands.TaskbarPinnedLaunchers,
			s_pinnedLaunchers,
			(allDesktopFiles, searchText,  chips, taskbarPinnedLaunchers, startMenuPinnedLaunchers) =>
			{
				var results = new LinkedList<StartMenuAppViewModel>();
				var index = 0;
				var isShowingSearchResults = chips[StartMenuChips.SearchResults].IsSelected;
				var isShowingPinned = chips[StartMenuChips.Pinned].IsSelected;
				var isShowingAllApps = chips[StartMenuChips.AllApps].IsSelected;
				var lowerCaseSearchText = searchText.ToLower();

				foreach (var f in allDesktopFiles)
				{
					var pinnedIndex = startMenuPinnedLaunchers.IndexOf(f.FilePath);
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

		ViewModel = Create(
			allAppsSelector,
			s_searchTextSelector,
			actionBarViewModelSelector,
			s_chipsSelector,
			(allApps, searchText, actionBarViewModel, chips) =>
			{
				return new StartMenuViewModel()
				{
					AllApps = allApps,
					SearchText = searchText,
					DisableDragAndDrop = searchText.Length > 0,
					ActionBarViewModel = actionBarViewModel,
					Chips = chips,
				};
			});
	}
}
