namespace Glimpse.Libraries.Configuration;

public static class ConfigurationStartupExtensions
{
	public static void AddGlimpseConfiguration(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<ConfigurationService>();

		var configSection = builder.Configuration.GetSection("Glimpse");
		builder.Services.Configure<GlimpseAppSettings>(configSection);
		builder.Services.PostConfigure<GlimpseAppSettings>(o =>
		{
			o.ApplicationName = string.IsNullOrEmpty(o.ApplicationName) ? "glimpse" : o.ApplicationName;
			o.ConfigurationFilePath = ExpandOrDefault(o.ConfigurationFilePath, $".config/{o.ApplicationName}/config.json");
			o.NotificationsFilePath = ExpandOrDefault(o.NotificationsFilePath, $".local/share/{o.ApplicationName}/notification-history.json");
			o.ClipboardHistoryFilePath = ExpandOrDefault(o.ClipboardHistoryFilePath, $".local/share/{o.ApplicationName}/clipboard-history.json");
		});
	}

	private static string ExpandOrDefault(string path, string defaultRelativePath)
	{
		if (string.IsNullOrEmpty(path))
		{
			path = Path.Combine("%HOME%", defaultRelativePath);
		}

		return Environment.ExpandEnvironmentVariables(path);
	}

	public static Task UseGlimpseConfiguration(this IHost host)
	{
		var configService = host.Services.GetRequiredService<ConfigurationService>();
		var appSettings = host.Services.GetRequiredService<IOptionsSnapshot<GlimpseAppSettings>>();
		configService.Initialize(appSettings.Value.ConfigurationFilePath);
		return Task.CompletedTask;
	}
}
