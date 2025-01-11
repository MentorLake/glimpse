using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.StatusNotifierWatcher;

public record StatusNotifierItemProperties
{
	public string AttentionIconName;
	public GdkPixbufHandle[] AttentionIconPixmap;
	public string AttentionMovieName;
	public string Category;
	public string IconName;
	public GdkPixbufHandle[] IconPixmap;
	public string Id;
	public bool ItemIsMenu;
	public string OverlayIconName;
	public GdkPixbufHandle[] OverlayIconPixmap;
	public string Status;
	public string Title;
	public string IconThemePath;
	public string MenuPath;
}
