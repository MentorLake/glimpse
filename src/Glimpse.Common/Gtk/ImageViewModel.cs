using Glimpse.Common.Images;

namespace Glimpse.Common.Gtk;

public record ImageViewModel
{
	public string IconNameOrPath { get; set; } = "";
	public IGlimpseImage Image { get; set; }
	public bool IsNull() => string.IsNullOrEmpty(IconNameOrPath) && Image == null;
	public static readonly ImageViewModel Empty = new();
}
