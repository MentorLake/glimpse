using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.Xorg.X11;

internal class X11DisplayServer(XLibAdaptorService xService) : IDisplayServer
{
	public GdkPixbufHandle TakeScreenshot(IWindowRef windowRef)
	{
		return xService.CaptureWindowScreenshot((XWindowRef)windowRef);
	}

	public void ToggleWindowVisibility(IWindowRef windowRef)
	{
		xService.ToggleWindowVisibility((XWindowRef)windowRef);
	}

	public void MakeWindowVisible(IWindowRef windowRef)
	{
		xService.MakeWindowVisible((XWindowRef)windowRef);
	}

	public void CloseWindow(IWindowRef windowRef)
	{
		xService.CloseWindow((XWindowRef)windowRef);
	}

	public void MaximizeWindow(IWindowRef windowRef)
	{
		xService.MakeWindowVisible((XWindowRef) windowRef);
		xService.MaximizeWindow((XWindowRef) windowRef);
	}

	public void MinimizeWindow(IWindowRef windowRef)
	{
		xService.MakeWindowVisible((XWindowRef) windowRef);
		xService.MinimizeWindow((XWindowRef) windowRef);
	}

	public void StartResizing(IWindowRef windowRef)
	{
		xService.MakeWindowVisible((XWindowRef) windowRef);
		xService.StartResizing((XWindowRef) windowRef);
	}

	public void StartMoving(IWindowRef windowRef)
	{
		xService.MakeWindowVisible((XWindowRef) windowRef);
		xService.StartMoving((XWindowRef) windowRef);
	}
}
