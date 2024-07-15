using Gdk;
using GLib;
using Glimpse.Common.Configuration;
using Glimpse.Common.System.Reflection;
using Gtk;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Host;

public class GlimpseGtkApplication(ILogger<GlimpseGtkApplication> logger, Application application, IOptions<GlimpseAppSettings> appSettings) : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken)
	{
		ExceptionManager.UnhandledException += args =>
		{
			logger.LogError(args.ExceptionObject.ToString());
			args.ExitApplication = false;
		};

		var commandLineArgs = Environment.GetCommandLineArgs();
		Application.Init(appSettings.Value.AppName, ref commandLineArgs);
		LoadCss();
		application.Register(Cancellable.Current);
		Task.Run(Application.Run);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		Application.Quit();
		return Task.CompletedTask;
	}

	private void LoadCss()
	{
		var display = Display.Default;
		var screen = display.DefaultScreen;
		var screenCss = new CssProvider();
		screenCss.LoadFromData(AppDomain.CurrentDomain.GetAssemblies().ConcatAllManifestFiles("css"));
		StyleContext.AddProviderForScreen(screen, screenCss, uint.MaxValue);
	}
}
