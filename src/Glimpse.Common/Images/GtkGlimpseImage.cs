using Gdk;

namespace Glimpse.Common.Images;

public class GtkGlimpseImage : IGlimpseImage
{
	public Pixbuf Pixbuf { get; init; }
	public int Width => Pixbuf.Width;
	public int Height => Pixbuf.Height;
	public void Dispose() => Pixbuf?.Dispose();

	public IGlimpseImage ScaleToFit(int maxWidth, int maxHeight)
	{
		return new GtkGlimpseImage { Pixbuf = Pixbuf.ScaleToFit(maxWidth, maxHeight) };
	}

	public IGlimpseImage Scale(int size) => new GtkGlimpseImage() { Pixbuf = Pixbuf.ScaleSimple(size, size, InterpType.Bilinear) };
}
