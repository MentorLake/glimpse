using System.Reflection;
using MentorLake.cairo;
using MentorLake.Gdk;
using MentorLake.GdkPixbuf;
using MentorLake.GLib;

namespace Glimpse.Libraries.Images;

public static class GdkPixbufFactory
{
	public static GdkPixbufHandle From(byte[] data, int depth, int width, int height, int rowStride)
	{
		var surface = cairoGlobalFunctions.ImageSurfaceCreateForData(data, depth == 24 ? cairo_format_t.CAIRO_FORMAT_RGB24 : cairo_format_t.CAIRO_FORMAT_ARGB32, width, height, rowStride);
		var pixbuf = GdkGlobalFunctions.PixbufGetFromSurface(surface, 0, 0, cairoGlobalFunctions.ImageSurfaceGetWidth(surface), cairoGlobalFunctions.ImageSurfaceGetHeight(surface));
		cairoGlobalFunctions.SurfaceDestroy(surface);
		return pixbuf;
	}

	public static GdkPixbufHandle From(byte[] data)
	{
		return GdkPixbufHandle.NewFromInline(data.Length, data, true);
	}

	public static GdkPixbufHandle From(byte[] data, int depth, int width, int height)
	{
		var surface = cairoGlobalFunctions.ImageSurfaceCreateForData(data, depth == 24 ? cairo_format_t.CAIRO_FORMAT_RGB24 : cairo_format_t.CAIRO_FORMAT_ARGB32, width, height, 4 * width);
		var pixbuf = GdkGlobalFunctions.PixbufGetFromSurface(surface, 0, 0, cairoGlobalFunctions.ImageSurfaceGetWidth(surface), cairoGlobalFunctions.ImageSurfaceGetHeight(surface));
		cairoGlobalFunctions.SurfaceDestroy(surface);
		return pixbuf;
	}

	public static GdkPixbufHandle From(string path)
	{
		return GdkPixbufHandle.NewFromFile(path);
	}

	public static GdkPixbufHandle From(byte[] data, bool hasAlpha, int bitsPerSample, int width, int height, int rowStride)
	{
		var bytes = GBytesHandle.New(data, (uint) data.Length);
		return GdkPixbufHandle.NewFromBytes(bytes, GdkColorspace.GDK_COLORSPACE_RGB, hasAlpha, bitsPerSample, width, height, rowStride);
	}

	public static GdkPixbufHandle FromResource(string resourceName)
	{
		var sourceAssembly = Assembly.GetCallingAssembly();
		using var resource = new StreamReader(sourceAssembly.GetManifestResourceStream(resourceName));
		var imageData = resource.ReadToEnd();
		var loader = GdkPixbufLoaderHandle.New();
		loader.Write(imageData.ToCharArray(), (uint) imageData.Length);
		GdkPixbufLoaderHandleExtensions.Close(loader);
		return loader.GetPixbuf();
	}
}
