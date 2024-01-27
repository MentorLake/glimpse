using Glimpse.Common.Images;

namespace Glimpse.Common.Freedesktop.Xorg;

public interface IDisplayServer
{
	void ToggleWindowVisibility(IWindowRef stateWindowRef);
	void MakeWindowVisible(IWindowRef windowRef);
	void MaximizeWindow(IWindowRef windowRef);
	void MinimizeWindow(IWindowRef windowRef);
	void StartResizing(IWindowRef windowRef);
	void StartMoving(IWindowRef windowRef);
	void CloseWindow(IWindowRef windowRef);
	IGlimpseImage TakeScreenshot(IWindowRef windowRef);
}
