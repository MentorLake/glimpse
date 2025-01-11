using System.Runtime.InteropServices;
using System.Text;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reflection;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.Gio;
using MentorLake.GLib;
using MentorLake.Gtk;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.UI;

public class GlimpseGtkApplication(GtkApplicationHandle application, IOptions<GlimpseAppSettings> appSettings, ILoggerFactory loggerFactory) : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken)
	{
		Task.Run(() =>
		{
			var argc = 0;
			var argv = new[] { appSettings.Value.ApplicationName };
			GtkGlobalFunctions.Init(ref argc, ref argv);
			SynchronizationContext.SetSynchronizationContext(GLibExt.SynchronizationContext);
			LoadCss();
			application.Register(GCancellableHandle.GetCurrent());

			GLibGlobalFunctions.LogSetDefaultHandler(static (domain, level, message, data) =>
			{
				var logLevel = level switch
				{
					GLogLevelFlags.G_LOG_FLAG_FATAL => LogLevel.Critical,
					GLogLevelFlags.G_LOG_LEVEL_CRITICAL => LogLevel.Critical,
					GLogLevelFlags.G_LOG_LEVEL_ERROR => LogLevel.Error,
					GLogLevelFlags.G_LOG_LEVEL_WARNING => LogLevel.Warning,
					GLogLevelFlags.G_LOG_LEVEL_DEBUG => LogLevel.Debug,
					_ => LogLevel.Information,
				};

				var factory = (ILoggerFactory)GCHandle.FromIntPtr(data).Target;
				var logger = factory.CreateLogger(domain);
				logger.Log(logLevel, message);
			}, GCHandle.ToIntPtr(GCHandle.Alloc(loggerFactory)));

			application.Run(0, null);
		}, cancellationToken);

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		application.Quit();
		return Task.CompletedTask;
	}

	private void LoadCss()
	{
		var display = GdkDisplayHandle.GetDefault();
		var screen = display.GetDefaultScreen();
		var screenCss = GtkCssProviderHandle.New();
		var allCss = AppDomain.CurrentDomain.GetAssemblies().ConcatAllManifestFiles("css");
		screenCss.LoadFromData(Encoding.UTF8.GetBytes(allCss), (uint) allCss.Length);
		GtkStyleContextHandle.AddProviderForScreen(screen, screenCss, uint.MaxValue);
	}
}
