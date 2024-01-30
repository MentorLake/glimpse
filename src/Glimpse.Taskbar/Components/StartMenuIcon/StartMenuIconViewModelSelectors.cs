using System.Collections.Immutable;
using Glimpse.Common.Gtk;
using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar.Components.StartMenuIcon;

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
