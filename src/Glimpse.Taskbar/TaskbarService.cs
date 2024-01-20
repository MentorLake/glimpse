using MentorLake.Redux;

namespace Glimpse.Taskbar;

public class TaskbarService(ReduxStore store)
{
	public void ToggleDesktopFilePinning(string desktopFileId)
	{
		store.Dispatch(new ToggleTaskbarPinningAction(desktopFileId));
	}
}
