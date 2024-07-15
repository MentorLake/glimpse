using GLib;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Host;

public static class GlimpseHostStartupExtensions
{
	public static Task UseGlimpseHost(this IHost host)
	{
		var orchestrator = host.Services.GetRequiredService<DisplayOrchestrator>();
		orchestrator.Init();
		orchestrator.WatchNotifications();
		return Task.CompletedTask;
	}

	public static void AddGlimpseHost(this IHostApplicationBuilder builder, string gtkApplicationIdentifier)
	{
		builder.Services.AddSingleton(_ => new Application(gtkApplicationIdentifier, ApplicationFlags.None));
		builder.Services.AddHostedService<GlimpseGtkApplication>();
		builder.Services.AddSingleton<GlimpseGtkApplication>();
		builder.Services.AddSingleton<DisplayOrchestrator>();
		builder.Services.AddSingleton<IReducerFactory, GlimpseGtkReducerFactory>();
	}
}
