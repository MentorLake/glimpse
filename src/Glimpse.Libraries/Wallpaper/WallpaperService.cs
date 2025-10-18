using System.Text.Encodings.Web;
using System.Text.Json;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.Xfce.SessionManagement;
using MentorLake.GLib;
using MentorLake.Redux;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Glimpse.Libraries.Wallpaper;

internal class WallpaperService(IHttpClientFactory clientFactory, IOptions<GlimpseAppSettings> appSettings, ILogger<WallpaperService> logger, ReduxStore store)
{
	internal async Task UpdateWallpaperAsync()
	{
		var wallpaperDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appSettings.Value.ApplicationName);
		var otherWallpaperInDirectory = Directory.GetFiles(wallpaperDirectory, "*.*").Where(f => Path.GetFileName(f).StartsWith("wallpaper_")).ToList();
		var imageUrl = await GetImageUrlAsync();
		var imageBytes = await GetImageAsync(imageUrl);
		var extension = Path.GetExtension(imageUrl);
		var wallpaperFileName = $"wallpaper_{DateTime.UtcNow.Ticks}{extension}";
		var wallpaperFullPath = Path.Join(wallpaperDirectory, wallpaperFileName);
		await File.WriteAllBytesAsync(wallpaperFullPath, imageBytes);
		SetWallpaper(wallpaperFullPath);
		DeleteOldWallpapers(otherWallpaperInDirectory);
	}

	private void DeleteOldWallpapers(List<string> otherWallpaperInDirectory)
	{
		foreach (var oldWallpaper in otherWallpaperInDirectory)
		{
			try
			{
				File.Delete(oldWallpaper);
			}
			catch (Exception e)
			{
				logger.LogWarning(e, $"Failed to delete wallpaper file ({oldWallpaper}).");
			}
		}
	}

	private void SetWallpaper(string wallpaperFullPath)
	{
		LibXfconfExterns.xfconf_init(IntPtr.Zero);
		var channel = LibXfconfExterns.xfconf_channel_get("xfce4-desktop");
		var propertiesTable = LibXfconfExterns.xfconf_channel_get_properties(channel, "/backdrop/screen0");
		var propertyNames = GHashTable.GetKeys(propertiesTable);
		var monitorProperties = propertyNames.ToListOfStrings();
		propertyNames.FreeFull();

		var monitorNames = monitorProperties.Select(m => m.Split("/", StringSplitOptions.RemoveEmptyEntries)[2]).Distinct().ToList();
		GHashTable.Destroy(propertiesTable);

		foreach (var monitorName in monitorNames)
		{
			LibXfconfExterns.xfconf_channel_set_string(channel,  $"/backdrop/screen0/{monitorName}/workspace0/last-image", wallpaperFullPath);
			LibXfconfExterns.xfconf_channel_set_uint(channel,  $"/backdrop/screen0/{monitorName}/workspace0/image-style", 5);
		}
	}

	private async Task<byte[]> GetImageAsync(string url)
	{
		using var imageClient = clientFactory.CreateClient();
		using var imageResponse = await imageClient.GetAsync(url);
		imageResponse.EnsureSuccessStatusCode();
		return await imageResponse.Content.ReadAsByteArrayAsync();
	}

	private async Task<string> GetImageUrlAsync()
	{
		var parameters = new Dictionary<string, string>()
		{
			["bcnt"] = "1",
			["country"] = "US",
			["fmt"] = "json",
			["locale"] = "en-US",
			["placement"] = "88000820"
		};

		var queryString = string.Join("&", parameters.Select(kv => $"{kv.Key}={UrlEncoder.Default.Encode(kv.Value)}"));
		using var request = new HttpRequestMessage(HttpMethod.Get, $"selection?{queryString}");
		using var jsonApiClient = clientFactory.CreateClient("iris");

		var response = await jsonApiClient.SendAsync(request);
		response.EnsureSuccessStatusCode();

		var outerDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
		var innerJsonString = outerDoc
			.RootElement
			.GetProperty("batchrsp")
			.GetProperty("items")[0]
			.GetProperty("item")
			.GetString();

		return JsonDocument.Parse(innerJsonString)
			.RootElement
			.GetProperty("ad")
			.GetProperty("landscapeImage")
			.GetProperty("asset")
			.GetString();
	}

	internal async Task SaveStateAsync(WallpaperState state)
	{
		await File.WriteAllTextAsync(StateFilePath, JsonSerializer.Serialize(state, WallpaperJsonSerializer.Default.WallpaperState));
	}

	internal async Task LoadStateAsync()
	{
		var state = new WallpaperState();

		if (!File.Exists(StateFilePath))
		{
			await File.WriteAllTextAsync(StateFilePath, JsonSerializer.Serialize(state, WallpaperJsonSerializer.Default.WallpaperState));
		}

		if (File.Exists(StateFilePath))
		{
			state = JsonSerializer.Deserialize(await File.ReadAllTextAsync(StateFilePath), WallpaperJsonSerializer.Default.WallpaperState);
		}

		await File.WriteAllTextAsync(StateFilePath, JsonSerializer.Serialize(state, WallpaperJsonSerializer.Default.WallpaperState));
		await store.Dispatch(new InternalWallpaperActions.UpdateStateAction(state));
	}

	private string StateFilePath
	{
		get
		{
			var stateDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appSettings.Value.ApplicationName);
			var stateFile = Path.Combine(stateDirectory, "wallpaper.json");
			return stateFile;
		}
	}
}
