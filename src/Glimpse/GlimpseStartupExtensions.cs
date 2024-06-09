using GLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

namespace Glimpse;

public static class GlimpseStartupExtensions
{
	public static Task UseGlimpseApplication(this IHost host)
	{
		return Task.CompletedTask;
	}

	public static void AddGlimpseApplication(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(_ =>
		{
			var app = new Gtk.Application("org.glimpse", ApplicationFlags.None);
			app.AddAction(new SimpleAction("OpenStartMenu", null));
			app.AddAction(new SimpleAction("LoadPanels", null));
			return app;
		});

		builder.Services.AddHostedService<GlimpseGtkApplication>();
		builder.Services.AddSingleton<GlimpseGtkApplication>();
	}
}
