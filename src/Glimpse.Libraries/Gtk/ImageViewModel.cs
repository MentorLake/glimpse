using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.Gtk;

public record ImageViewModel
{
	public string IconNameOrPath { get; set; } = "";
	public GdkPixbufHandle Image { get; set; }
	public bool IsNull() => string.IsNullOrEmpty(IconNameOrPath) && Image == null;
	public static readonly ImageViewModel Empty = new();
}
