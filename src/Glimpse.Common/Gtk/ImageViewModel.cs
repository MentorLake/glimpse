using Glimpse.Common.Images;

namespace Glimpse.UI.State;

public record ImageViewModel
{
	public string IconNameOrPath { get; set; } = "";
	public IGlimpseImage Image { get; set; }
}
