using System;
using System.CommandLine;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Glimpse.UI;
using Glimpse.Libraries.Accounts;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.DBus;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.StatusNotifierWatcher;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xfce.SessionManagement;
using Glimpse.Libraries.Xorg;
using Glimpse.Services;
using MentorLake.Gio;
using MentorLake.GLib;
using MentorLake.Gtk;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Glimpse;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
		var installCommand = new Command("-install", "Install Glimpse");
		installCommand.AddAlias("-i");
		installCommand.SetHandler(_ => Installation.RunScript(Installation.InstallScriptResourceName));

		var uninstallCommand = new Command("-uninstall", "Uninstall Glimpse");
		uninstallCommand.AddAlias("-u");
		uninstallCommand.SetHandler(_ => Installation.RunScript(Installation.UninstallScriptResourceName));

		var version = Assembly.GetExecutingAssembly().GetName().Version;
		var formattedVersion = $"{version.Major}.{version.Minor:D2}.{version.Build:D2}.{version.Revision}";
		var updateCommand = new Command("-update", "Download and install the latest version");
		updateCommand.SetHandler(_ => Installation.RunScript(Installation.UpdateScriptResourceName, formattedVersion));

		var rootCommand = new RootCommand("Glimpse");
		rootCommand.AddCommand(installCommand);
		rootCommand.AddCommand(uninstallCommand);
		rootCommand.AddCommand(updateCommand);
		rootCommand.AddOption(new Option<string>("--sm-client-id"));
		rootCommand.SetHandler(async c => c.ExitCode = await RunGlimpseAsync());

		return await rootCommand.InvokeAsync(args);
	}

	private static async Task<int> RunGlimpseAsync()
	{
		var configuration = new ConfigurationBuilder()
			.AddEnvironmentVariables()
			.AddJsonStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("appsettings.json")!)
			.Build();

		var appName = configuration.GetValue<string>("Glimpse:ApplicationName");
		using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddConsole().AddJournal(o => o.SyslogIdentifier = appName));
		var bootstrapLogger = bootstrapLoggerFactory.CreateLogger(appName);

		try
		{
			AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => bootstrapLogger.LogError(eventArgs.ExceptionObject.ToString());
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
			builder.AddUI("org.glimpse");

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

			GLibGlobalFunctions.SetPrgname(appSettings.Value.ApplicationName);

			await host.UseXSessionManagement();
			await host.UseDBus();
			await host.UseDesktopFiles();
			await host.UseXorg();
			await host.UseGlimpseConfiguration();
			await host.UseAccounts();
			await host.UseStatusNotifier();
			await host.UseServices(host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>().Value.ConfigurationFilePath);
			await host.UseUI();

			await store.Dispatch(new InitializeStoreAction());

			var orchestrator = host.Services.GetRequiredService<DisplayOrchestrator>();
			orchestrator.WatchMonitorChanges();

			var app = host.Services.GetRequiredService<GtkApplicationHandle>();
			app.Signal_Startup().Take(1).Subscribe(_ =>
			{
#if !DEBUG
				var sessionClient = host.Services.GetRequiredService<OrgXfceSessionClient>();
				sessionClient.Register(Path.Join(AppContext.BaseDirectory, appSettings.Value.ApplicationName), appSettings.Value.Xfce);
#endif
			});

			await host.RunAsync();
			return 0;
		}
		catch (Exception e)
		{
			bootstrapLogger.LogError(e.ToString());
			return 1;
		}
	}
}
