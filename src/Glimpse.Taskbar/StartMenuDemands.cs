using System.Collections.Immutable;
using Glimpse.StartMenu;
using MentorLake.Redux.Selectors;

namespace Glimpse.Taskbar;

public class StartMenuDemands(TaskbarService taskbarService) : IStartMenuDemands
{
	public void ToggleDesktopFilePinning(string desktopFileId) => taskbarService.ToggleDesktopFilePinning(desktopFileId);
	public ISelector<ImmutableList<string>> TaskbarPinnedLaunchers => TaskbarSelectors.PinnedLaunchers;
}
