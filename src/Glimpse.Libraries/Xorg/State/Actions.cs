using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.Xorg.State;

public record AddWindowAction(WindowProperties WindowProperties);

public class UpdateWindowAction
{
	public WindowProperties WindowProperties { get; set; }
}

public class RemoveWindowAction
{
	public WindowProperties WindowProperties { get; set; }
}

public class TakeScreenshotAction
{
	public IEnumerable<IWindowRef> Windows { get; set; }
}

public class UpdateScreenshotsAction
{
	public Dictionary<ulong, GdkPixbufHandle> Screenshots { get; set; }
}

public class WindowFocusedChangedAction
{
	public IWindowRef WindowRef { get; set; }
}
