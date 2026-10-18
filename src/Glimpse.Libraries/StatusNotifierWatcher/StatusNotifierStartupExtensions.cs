namespace Glimpse.Libraries.StatusNotifierWatcher;

public static class StatusNotifierStartupExtensions
{
	public static Task UseStatusNotifier(this IHost host)
	{
		return Task.CompletedTask;
	}

	public static void AddStatusNotifier(this IHostApplicationBuilder builder)
	{
		var services = builder.Services;
		services.AddSingleton<StatusNotifierWatcherService>();
		services.AddSingleton<OrgKdeStatusNotifierWatcher>();
		services.AddTransient<IReducerFactory, StatusNotifierWatcherReducer>();
	}
}
