using System.Collections.Immutable;
using Glimpse.Libraries.Accounts;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System;
using Glimpse.Services.StartMenu;
using Glimpse.Services.Taskbar;
using Glimpse.UI.Components.Shared;
using MentorLake.Redux;
using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.UI.Components.StartMenu;

public static class StartMenuViewModelSelectors
{
	private static readonly ISelector<ActionBarViewModel> s_actionBarViewModelSelector = Create(
		StartMenuSelectors.PowerButtonCommand,
		StartMenuSelectors.SettingsButtonCommand,
		StartMenuSelectors.UserSettingsCommand,
		AccountSelectors.UserIconPath,
		(powerButtonCommand, settingsButtonCommand, userSettingsCommand, userIconPath) => new ActionBarViewModel()
		{
			PowerButtonCommand = powerButtonCommand,
			SettingsButtonCommand = settingsButtonCommand,
			UserSettingsCommand = userSettingsCommand,
			UserIconPath = userIconPath
		});

	private static readonly ISelector<ImmutableDictionary<string, StartMenuAppViewModel>> s_appIconViewModelsSelector = Create<DataTable<string, DesktopFile>, ImmutableList<string>, ImmutableList<string>, ImmutableDictionary<string, StartMenuAppViewModel>>(
			DesktopFileSelectors.DesktopFiles,
			TaskbarSelectors.PinnedLaunchers,
			StartMenuSelectors.PinnedLaunchers,
			(previous, allDesktopFiles, taskbarPinnedLaunchers, startMenuPinnedLaunchers) =>
			{
				var results = new LinkedList<StartMenuAppViewModel>();
				var index = -1;

				foreach (var f in allDesktopFiles.ById.Values.OrderBy(f => f.Name))
				{
					index++;
					var pinnedIndex = startMenuPinnedLaunchers.IndexOf(f.FilePath);
					var taskbarIndex = taskbarPinnedLaunchers.IndexOf(f.FilePath);
					var priorResult = previous.HasValue && previous.Value.Result.TryGetValue(f.FilePath, out var value) ? value : null;

					if (priorResult != null && priorResult.DesktopFile == f && priorResult.PinnedIndex == pinnedIndex && priorResult.IsPinnedToTaskbar == taskbarIndex > -1)
					{
						results.AddLast(priorResult);
						continue;
					}

					var appViewModel = new StartMenuAppViewModel
					{
						DesktopFile = f,
						Icon = new IconInfo() { Name = !f.IconName.StartsWith("/") ? f.IconName : "", Path = f.IconName.StartsWith("/") ? f.IconName : "" },
						IsPinnedToTaskbar = taskbarIndex > -1,
						IsPinnedToStartMenu = pinnedIndex > -1,
						Index = index,
						PinnedIndex = pinnedIndex,
						ActionIcons = f.Actions.ToDictionary(a => a.ActionName, a => new ImageViewModel() { IconNameOrPath = f.IconName })
					};

					if (priorResult != null)
					{
						appViewModel.ActionIcons = appViewModel.ActionIcons.Values.SequenceEqual(priorResult.ActionIcons.Values, FuncEqualityComparer<ImageViewModel>.Create((x, y) => x.IconNameOrPath == y.IconNameOrPath))
							? priorResult.ActionIcons
							: appViewModel.ActionIcons;

						appViewModel.Icon = appViewModel.Icon.Name == priorResult.Icon.Name && appViewModel.Icon.Path == priorResult.Icon.Path
							? priorResult.Icon
							: appViewModel.Icon;
					}

					results.AddLast(appViewModel);
				}

				return results.ToImmutableDictionary(i => i.DesktopFile.FilePath, i => i);
			});

	private static readonly ISelector<ImmutableList<string>> s_searchSelector = Create(
		s_appIconViewModelsSelector,
		StartMenuSelectors.SearchTextSelector,
		(appIconViewModels, searchText) =>
	{
		var results = ImmutableList<string>.Empty;

		foreach (var f in appIconViewModels.Values)
		{
			if (!string.IsNullOrEmpty(searchText) && searchText.ToLower().AllCharactersIn(f.DesktopFile.Name.ToLower()))
			{
				results = results.Add(f.DesktopFile.Name);
			}
		}

		return results;
	});

	public static readonly ISelector<StartMenuViewModel> ViewModel = Create(
		s_appIconViewModelsSelector,
		s_searchSelector,
		s_actionBarViewModelSelector,
		(allApps, searchResults, actionBarViewModel) => new StartMenuViewModel()
		{
			AllApps = allApps,
			SearchResults = searchResults,
			ActionBarViewModel = actionBarViewModel
		});
}
