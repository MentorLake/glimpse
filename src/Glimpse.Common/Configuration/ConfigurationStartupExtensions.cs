using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Glimpse.Common.Configuration;

public static class ConfigurationStartupExtensions
{
	public static void AddGlimpseConfiguration(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<ConfigurationService>();

		var configSection = builder.Configuration.GetSection("Glimpse");
		builder.Services.Configure<GlimpseAppSettings>(configSection);
		builder.Services.PostConfigure<GlimpseAppSettings>(o =>
		{
			o.ConfigurationFilePath = Environment.ExpandEnvironmentVariables(o.ConfigurationFilePath);
			o.NotificationsFilePath = Environment.ExpandEnvironmentVariables(o.NotificationsFilePath);
		});
	}

	public static Task UseGlimpseConfiguration(this IHost host)
	{
		var configService = host.Services.GetRequiredService<ConfigurationService>();
		var appSettings = host.Services.GetRequiredService<IOptionsSnapshot<GlimpseAppSettings>>();
		configService.Initialize(appSettings.Value.ConfigurationFilePath);
		return Task.CompletedTask;
	}
}
