using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.Libraries.Wallpaper;

[JsonSerializable(typeof(WallpaperConfiguration))]
[JsonSerializable(typeof(WallpaperState))]
internal partial class WallpaperJsonSerializer : JsonSerializerContext
{
	public static WallpaperJsonSerializer Instance { get; } = new(
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
