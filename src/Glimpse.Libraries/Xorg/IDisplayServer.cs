using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.Xorg;

public interface IDisplayServer
{
	void ToggleWindowVisibility(IWindowRef stateWindowRef);
	void MakeWindowVisible(IWindowRef windowRef);
	void MaximizeWindow(IWindowRef windowRef);
	void MinimizeWindow(IWindowRef windowRef);
	void StartResizing(IWindowRef windowRef);
	void StartMoving(IWindowRef windowRef);
	void CloseWindow(IWindowRef windowRef);
	GdkPixbufHandle TakeScreenshot(IWindowRef windowRef);
}
