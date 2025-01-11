using System.Collections.Immutable;
using Glimpse.Libraries.Gtk;
using Glimpse.Services.Taskbar;
using Glimpse.UI.Components.Shared;
using MentorLake.Redux.Selectors;

namespace Glimpse.UI.Components.StartMenuIcon;

internal static class StartMenuIconViewModelSelectors
{
	internal static readonly ISelector<StartMenuIconViewModel> s_viewModel = SelectorFactory.Create(
		TaskbarSelectors.s_contextMenu,
		TaskbarSelectors.s_startMenuLaunchIconName,
		(contextMenuItems, startMenuLaunchIconName) => new StartMenuIconViewModel()
		{
			ContextMenuItems = contextMenuItems
				.Select(i => new StartMenuIconContextMenuItem()
				{
					DisplayText = i.DisplayText,
					Icon = new ImageViewModel() { IconNameOrPath = i.Icon },
					Executable = i.Executable,
					Arguments = i.Arguments
				})
				.ToImmutableList(),
			StartMenuLaunchIconName = startMenuLaunchIconName
		});
}
