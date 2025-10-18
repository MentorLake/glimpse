using System.Text.Json;
using System.Text.Json.Nodes;

namespace Glimpse.Libraries.Wallpaper;

internal record WallpaperConfiguration
{
	public const string DefaultCron = "0 0 * * *";
	public const string ConfigKey = "Wallpaper";
	public static readonly WallpaperConfiguration Empty = new();

	public string Cron { get; set; } = DefaultCron;
	public bool IsEnabled { get; set; }

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(WallpaperConfiguration), WallpaperJsonSerializer.Instance)?.AsObject();
	}

	public static WallpaperConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(WallpaperConfiguration), WallpaperJsonSerializer.Instance) as WallpaperConfiguration;
	}
}
