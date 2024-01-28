using Glimpse.Common.Images;

namespace Glimpse.Common.StatusNotifierWatcher;

public record StatusNotifierItemProperties
{
	public string AttentionIconName;
	public IGlimpseImage[] AttentionIconPixmap;
	public string AttentionMovieName;
	public string Category;
	public string IconName;
	public IGlimpseImage[] IconPixmap;
	public string Id;
	public bool ItemIsMenu;
	public string OverlayIconName;
	public IGlimpseImage[] OverlayIconPixmap;
	public string Status;
	public string Title;
	public string IconThemePath;
	public string MenuPath;
}
