using System.Reflection;
using Cairo;
using Gdk;

namespace Glimpse.Common.Images;

public static class GlimpseImageFactory
{
	public static IGlimpseImage From(IntPtr byteArrayPointer, int depth, int width, int height, int rowStride)
	{
		var sourceImageSurface = new ImageSurface(byteArrayPointer, depth == 24 ? Format.RGB24 : Format.Argb32, width, height, rowStride);
		var image = new GtkGlimpseImage { Pixbuf = new Pixbuf(sourceImageSurface, 0, 0, sourceImageSurface.Width, sourceImageSurface.Height) };
		sourceImageSurface.Dispose();
		return image;
	}

	public static IGlimpseImage From(byte[] data)
	{
		using var memoryStream = new MemoryStream(data);
		return new GtkGlimpseImage { Pixbuf = new Pixbuf(memoryStream) };
	}

	public static IGlimpseImage From(byte[] data, int depth, int width, int height)
	{
		var surface = new ImageSurface(data, depth == 24 ? Format.RGB24 : Format.Argb32, width, height, 4 * width);
		var buffer = new Pixbuf(surface, 0, 0, width, height);
		surface.Dispose();
		return new GtkGlimpseImage { Pixbuf = buffer };
	}

	public static IGlimpseImage From(string path)
	{
		return new GtkGlimpseImage() { Pixbuf = new Pixbuf(path) };
	}

	public static IGlimpseImage From(Pixbuf pixbuf)
	{
		return new GtkGlimpseImage() { Pixbuf = pixbuf };
	}

	public static IGlimpseImage From(byte[] data, bool hasAlpha, int bitsPerSample, int width, int height, int rowStride)
	{
		return new GtkGlimpseImage { Pixbuf = new Pixbuf(data, Colorspace.Rgb, hasAlpha, bitsPerSample, width, height, rowStride) };
	}

	public static IGlimpseImage FromResource(string resourceName)
	{
		var sourceAssembly = Assembly.GetCallingAssembly();
		using var loader = new PixbufLoader(sourceAssembly, resourceName);
		return new GtkGlimpseImage { Pixbuf = loader.Pixbuf };
	}
}
