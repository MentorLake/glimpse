using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.System.Reactive;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace Glimpse.Libraries.Wallpaper;

public static class WallpaperStartupExtensions
{
	public static void AddWallpaper(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<WallpaperService>();
		builder.Services.AddTransient<IReducerFactory, WallpaperReducers>();
		builder.Services.AddTransient<IEffectsFactory, WallpaperEffects>();
		builder.Services.AddHttpClient("iris", client => client.BaseAddress = new Uri("https://fd.api.iris.microsoft.com/v4/api/"));
	}

	public static async Task UseWallpaperAsync(this IHost host)
	{
		var services = host.Services;
		var store = services.GetRequiredService<ReduxStore>();
		var logger = services.GetRequiredService<ILogger<WallpaperConfiguration>>();
		var wallpaperService =  services.GetRequiredService<WallpaperService>();
		var configurationService = services.GetRequiredService<ConfigurationService>();
		configurationService.AddIfNotExists(WallpaperConfiguration.ConfigKey, WallpaperConfiguration.Empty.ToJsonElement());

		configurationService
			.ObserveChange(WallpaperConfiguration.ConfigKey)
			.Subscribe(c =>
			{
				var config = WallpaperConfiguration.From(c);

				if (CrontabSchedule.TryParse(config.Cron) == null)
				{
					logger.LogWarning($"Invalid crontab expression [{config.Cron}] found in configuration file.  Defaulting to every 24 hours.");
					config.Cron = WallpaperConfiguration.DefaultCron;
				}

				store.Dispatch(new InternalWallpaperActions.UpdateConfigurationAction(config));
			});

		await wallpaperService.LoadStateAsync();
		TimerFactory.OneMinuteTimer.Subscribe(_ => store.Dispatch(new InternalWallpaperActions.WallpaperTickAction()));
	}
}
