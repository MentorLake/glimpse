using System.Reactive.Linq;
using System.Reflection;
using Glimpse.UI;
using Glimpse.Libraries.Accounts;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.DBus;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.StatusNotifierWatcher;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xfce.SessionManagement;
using Glimpse.Libraries.Xorg;
using Glimpse.Services;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.Gio;
using MentorLake.Gtk;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Tests;

public class Program
{
	public static async Task Main(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.AddEnvironmentVariables()
			.AddJsonStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("appsettings.json")!)
			.Build();

		var appName = configuration.GetValue<string>("Glimpse:ApplicationName");
		using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddConsole().AddJournal(o => o.SyslogIdentifier = appName));
		var bootstrapLogger = bootstrapLoggerFactory.CreateLogger(appName);

		var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder([]);
		builder.Configuration.AddConfiguration(configuration);
		builder.Services.AddSingleton<ReduxStore>();
		builder.Services.AddLogging(b => b.AddConsole().AddJournal(o => o.SyslogIdentifier = appName));
		builder.AddReactiveTimers();
		builder.AddDBus();
		builder.AddXorg();
		builder.AddDesktopFiles();
		builder.AddStatusNotifier();
		builder.AddXSessionManagement();
		builder.AddAccounts();
		builder.AddGlimpseConfiguration();
		builder.AddServices();
		builder.AddUI("org.glimpsetests");

		var host = builder.Build();
		var store = host.Services.GetRequiredService<ReduxStore>();
		store.RegisterReducers(host.Services.GetServices<IReducerFactory>().ToArray());
		store.RegisterReducers(host.Services.GetServices<FeatureReducerCollection>().ToArray());
		store.RegisterEffects(host.Services.GetServices<IEffectsFactory>().ToArray()
			.SelectMany(e => e.Create())
			.Select(oldEffect => new Effect()
			{
				Run = _ => oldEffect.Run(store.Actions).Do(_ => { }, exception => bootstrapLogger.LogError(exception.ToString())),
				Config = oldEffect.Config
			})
			.ToArray());

		var appSettings = host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>();

		await host.UseXSessionManagement(Environment.CurrentDirectory, appSettings.Value.Xfce);
		await host.UseDBus();
		await host.UseDesktopFiles();
		await host.UseXorg();
		await host.UseGlimpseConfiguration();
		await host.UseAccounts();
		await host.UseStatusNotifier();
		await host.UseServices(host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>().Value.ConfigurationFilePath);
		await host.UseUI();

		var orchestrator = host.Services.GetRequiredService<DisplayOrchestrator>();
		var application = host.Services.GetRequiredService<GtkApplicationHandle>();

		application
			.Signal_Startup()
			.ObserveOn(GLibExt.Scheduler)
			.Subscribe(_ =>
			{
				orchestrator.CreatePanel(GdkDisplayHandle.GetDefault().GetMonitors().First(m => !m.IsPrimary()), 100);
			});

		await host.RunAsync();
	}
}
