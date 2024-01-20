using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Freedesktop;
using Glimpse.Freedesktop.SystemTray;
using MentorLake.Redux.Effects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.UI.Components.SystemTray;

public static class SystemTrayStartupExtensions
{
	public static async Task UseSystemTray(this IHost host)
	{
		var container = host.Services;
		await container.GetRequiredService<DBusSystemTrayService>().InitializeAsync();
	}

	public static void AddSystemTray(this IHostApplicationBuilder builder)
	{
		builder.Services.AddInstance(SystemTrayItemStateReducers.Reducers);
		builder.Services.AddSingleton<IEffectsFactory, SystemTrayItemStateEffects>();
		builder.Services.AddTransient<DBusSystemTrayService>();
		builder.Services.AddTransient<SystemTrayBox>();
	}
}
