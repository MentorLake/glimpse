using Glimpse.Libraries.System;
using MentorLake.GdkPixbuf;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared;

public class IconInfo
{
	public string Name { get; set; }
	public string Path { get; set; }
	public string Key { get; set; }

	public string Id => Key.Or(Path).Or(Name);

	public static IconInfo FromNameOrPath(string nameOrPath)
	{
		if (string.IsNullOrEmpty(nameOrPath))
		{
			return new() { Name = "image-missing" };
		}

		if (nameOrPath.StartsWith("/"))
		{
			return new() { Path = nameOrPath };
		}

		return new() { Name = nameOrPath };
	}
}

public class CachedIconSet
{
	public Dictionary<int, Icon> Sizes { get; set; } = new();
}

public class Icon
{
	public required IconInfo Info { get; init; }
	public long Timestamp { get; } = DateTime.UtcNow.Ticks;
	public required GdkPixbufHandle Image { get; init; }
	public required int Size { get; init; }
}

public class IconManager
{
	private readonly GtkIconThemeHandle _theme = GtkIconThemeHandle.GetDefault();
	private readonly Dictionary<string, CachedIconSet> _cache = new();
	private const string ImageMissingIconName = "image-missing";

	private static IconManager s_instance;
	public static IconManager GetDefault() => s_instance ??= new IconManager();

	public Icon GetIcon(IconInfo info, int size)
	{
		try
		{
			if (!string.IsNullOrEmpty(info.Path))
			{
				return LoadFromPath(info, size);
			}

			if (!string.IsNullOrEmpty(info.Name))
			{
				return LoadFromName(info, size);
			}

			return LoadFromKey(info, size);
		}
		catch (Exception)
		{
			return LoadFromName(new IconInfo() { Name = ImageMissingIconName }, size);
		}
	}

	public bool TryGetUpdatedIcon(IconInfo newInfo, int size, Icon? priorIcon, out Icon updatedIcon)
	{
		updatedIcon = GetIcon(newInfo, size);
		return updatedIcon.Timestamp != priorIcon?.Timestamp;
	}

	private Icon LoadFromKey(IconInfo info, int size)
	{
		if (!_cache.ContainsKey(info.Key))
		{
			return LoadFromName(new IconInfo() { Name = ImageMissingIconName }, size);
		}

		if (_cache[info.Key].Sizes.ContainsKey(size))
		{
			return _cache[info.Key].Sizes[size];
		}

		var largestIcon = _cache[info.Key].Sizes[_cache[info.Key].Sizes.Keys.Max()];
		var scaledIcon = largestIcon.Image.ScaleSimple(size, size, GdkInterpType.GDK_INTERP_BILINEAR);
		_cache[info.Key].Sizes.Add(size, new Icon() { Image = scaledIcon, Size = size, Info = new IconInfo() { Key = info.Key } });
		return _cache[info.Key].Sizes[size];
	}

	private Icon LoadFromName(IconInfo info, int size)
	{
		if (_cache.ContainsKey(info.Name) && _cache[info.Name].Sizes.ContainsKey(size))
		{
			return _cache[info.Name].Sizes[size];
		}

		if (!_cache.ContainsKey(info.Name))
		{
			_cache[info.Name] = new CachedIconSet();
		}

		var image = _theme.LoadIconForScale(info.Name, size, 1, GtkIconLookupFlags.GTK_ICON_LOOKUP_FORCE_SIZE);
		_cache[info.Name].Sizes.Add(size, new Icon() { Image = image, Size = size, Info = new IconInfo() { Name = info.Name } });
		return _cache[info.Name].Sizes[size];
	}

	private Icon LoadFromPath(IconInfo info, int size)
	{
		if (_cache.ContainsKey(info.Path) && _cache[info.Path].Sizes.ContainsKey(size))
		{
			return _cache[info.Path].Sizes[size];
		}

		if (!_cache.ContainsKey(info.Path))
		{
			_cache[info.Path] = new CachedIconSet();
		}

		var image = GdkPixbufHandle.NewFromFileAtSize(info.Path, size, size);
		_cache[info.Path].Sizes.Add(size, new Icon() { Image = image, Size = size, Info = new IconInfo() { Path = info.Path } });
		return _cache[info.Path].Sizes[size];
	}

	public Icon AddKeyedIcon(string key, GdkPixbufHandle pixbuf)
	{
		if (!_cache.ContainsKey(key))
		{
			_cache[key] = new();
		}

		var iconUpdated = _cache[key].Sizes.ContainsKey(pixbuf.GetWidth());
		_cache[key].Sizes[pixbuf.GetWidth()] = new Icon() { Image = pixbuf, Size = pixbuf.GetWidth(), Info = new IconInfo() { Key = key } };

		if (iconUpdated)
		{
			// Dispatch action that the icon was updated
		}

		return _cache[key].Sizes[pixbuf.GetWidth()];
	}

	public void RemoveKeyedIcon(string key)
	{
		_cache.Remove(key);
	}
}
