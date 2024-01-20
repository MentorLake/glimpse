using Glimpse.StartMenu;
using Glimpse.Taskbar;

namespace Glimpse.UI;

public class StartMenuDemands(TaskbarService taskbarService) : IStartMenuDemands
{
	public void ToggleDesktopFilePinning(string desktopFileId) => taskbarService.ToggleDesktopFilePinning(desktopFileId);
}
