namespace Glimpse.Libraries.Wallpaper;

internal record WallpaperState
{
	public DateTime LastUpdate { get; set; } = DateTime.MinValue;
}
