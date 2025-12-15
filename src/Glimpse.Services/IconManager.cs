using Glimpse.Libraries.System;
using Glimpse.Libraries.Xorg.State;
using MentorLake.GdkPixbuf;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.Services;

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

public class IconManager(ReduxStore store)
{
	private readonly Lazy<GtkIconThemeHandle> _theme = new(GtkIconThemeHandle.GetDefault);
	private readonly Dictionary<string, CachedIconSet> _cache = new();
	private const string ImageMissingIconName = "image-missing";

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
		var windows = XorgSelectors.Windows.Apply(store.State);

		if (!_cache.ContainsKey(info.Key) && windows.AllIds.Any(id => id.ToString() == info.Id))
		{
			_cache[info.Key] = new();

			windows.Get(ulong.Parse(info.Key))
				.Icons
				.Select(i => new Icon() { Image = i, Info = new() { Key = info.Key }, Size = i.GetWidth() })
				.DistinctBy(i => i.Size)
				.ToList()
				.ForEach(i => _cache[info.Key].Sizes.Add(i.Size, i));
		}

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

		var image = _theme.Value.LoadIconForScale(info.Name, size, 1, GtkIconLookupFlags.GTK_ICON_LOOKUP_FORCE_SIZE);
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

	public void RemoveKeyedIcon(string key)
	{
		_cache.Remove(key);
	}
}
