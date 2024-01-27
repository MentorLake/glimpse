using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Common.Configuration;

public static class ConfigurationStartupExtensions
{
	public static void AddGlimpseConfiguration(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<ConfigurationService>();
	}

	public static Task UseGlimpseConfiguration(this IHost host)
	{
		var configService = host.Services.GetRequiredService<ConfigurationService>();
		configService.Initialize();
		return Task.CompletedTask;
	}
}
