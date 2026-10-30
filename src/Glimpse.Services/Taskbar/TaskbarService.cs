using MentorLake.Redux.Thunks;

namespace Glimpse.Services.Taskbar;

public class TaskbarService(ReduxStore store)
{
	public void ToggleDesktopFilePinning(string desktopFileId)
	{
		store.Using<TaskbarThunkApi>().Dispatch(TaskbarThunks.TogglePinning, desktopFileId);
	}
}
