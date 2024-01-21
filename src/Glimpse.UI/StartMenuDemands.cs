using System.Collections.Immutable;
using Glimpse.StartMenu;
using Glimpse.Taskbar;
using MentorLake.Redux.Selectors;

namespace Glimpse.UI;

public class StartMenuDemands(TaskbarService taskbarService) : IStartMenuDemands
{
	public void ToggleDesktopFilePinning(string desktopFileId) => taskbarService.ToggleDesktopFilePinning(desktopFileId);
	public ISelector<ImmutableList<string>> TaskbarPinnedLaunchers => TaskbarStateSelectors.PinnedLaunchers;
}
