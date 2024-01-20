using Gdk;

namespace Glimpse.Common.Images;

public interface IGlimpseImage : IDisposable
{
	int Width { get; }
	int Height { get; }
	IGlimpseImage ScaleToFit(int maxHeight, int maxWidth);
	IGlimpseImage Scale(int size);
	Pixbuf Pixbuf { get; }
}
