using Gdk;

namespace Glimpse.Common.Images;

public static class ImageHelper
{
	public static Pixbuf ScaleToFit(this Pixbuf pixbuf, int maxWidth, int maxHeight)
	{
		double width = pixbuf.Width;
		double height = pixbuf.Height;
		var aspectRatio = width / height;

		int newWidth, newHeight;

		if (width > height)
		{
			newWidth = maxWidth;
			newHeight = (int)(maxWidth / aspectRatio);

			if (newHeight > maxHeight)
			{
				newHeight = maxHeight;
				newWidth = (int)(newHeight * aspectRatio);
			}
		}
		else
		{
			newHeight = maxHeight;
			newWidth = (int)(maxHeight * aspectRatio);

			if (newWidth > maxWidth)
			{
				newWidth = maxWidth;
				newHeight = (int)(newWidth * aspectRatio);
			}
		}

		if (pixbuf.Width == newWidth && pixbuf.Height == newHeight)
		{
			return pixbuf;
		}

		return pixbuf.ScaleSimple(newWidth, newHeight, InterpType.Bilinear);
	}

	public static byte[] ConvertArgbToRgba(byte[] data, int width, int height)
	{
		var newArray = new byte[data.Length];
		Array.Copy(data, newArray, data.Length);

		for (var i = 0; i < 4 * width * height; i += 4)
		{
			var alpha = newArray[i];
			newArray[i] = newArray[i + 1];
			newArray[i + 1] = newArray[i + 2];
			newArray[i + 2] = newArray[i + 3];
			newArray[i + 3] = alpha;
		}

		return newArray;
	}
}
