using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar.Components.StartMenuIcon;

internal static class StartMenuIconViewModelSelectors
{
	internal static readonly ISelector<StartMenuIconViewModel> s_viewModel = SelectorFactory.Create(
		TaskbarSelectors.s_contextMenu,
		TaskbarSelectors.s_startMenuLaunchIconName,
		(contextMenuItems, startMenuLaunchIconName) => new StartMenuIconViewModel()
		{
			ContextMenuItems = contextMenuItems,
			StartMenuLaunchIconName = startMenuLaunchIconName
		});
}
