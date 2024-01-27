using Glimpse.Common.Images;

namespace Glimpse.Common.Freedesktop.Xorg.State;

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
	public Dictionary<ulong, IGlimpseImage> Screenshots { get; set; }
}

public class WindowFocusedChangedAction
{
	public IWindowRef WindowRef { get; set; }
}
