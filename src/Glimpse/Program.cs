using System.CommandLine;
using System.Reactive.Linq;
using System.Reflection;
using Glimpse.Common.Accounts;
using Glimpse.Common.Configuration;
using Glimpse.Common.DBus;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.StatusNotifierWatcher;
using Glimpse.Common.System.Reactive;
using Glimpse.Common.Xfce.SessionManagement;
using Glimpse.Common.Xorg;
using Glimpse.Host;
using Glimpse.Notifications;
using Glimpse.SidePane;
using Glimpse.StartMenu;
using Glimpse.SystemTray;
using Glimpse.Taskbar;
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
		var installCommand = new Command("install", "Install Glimpse");
		installCommand.AddAlias("i");
		installCommand.SetHandler(_ => Installation.RunScript(Installation.InstallScriptResourceName));

		var uninstallCommand = new Command("uninstall", "Uninstall Glimpse");
		uninstallCommand.AddAlias("u");
		uninstallCommand.SetHandler(_ => Installation.RunScript(Installation.UninstallScriptResourceName));

		var rootCommand = new RootCommand("Glimpse");
		rootCommand.AddCommand(installCommand);
		rootCommand.AddCommand(uninstallCommand);
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
			builder.AddReactiveTimers();
			builder.AddDBus();
			builder.AddXorg();
			builder.AddDesktopFiles();
			builder.AddGlimpseHost("org.glimpse");
			builder.AddStatusNotifier();
			builder.AddXSessionManagement();
			builder.AddAccounts();
			builder.AddGlimpseConfiguration();
			builder.AddTaskbar();
			builder.AddSystemTray();
			builder.AddStartMenu<StartMenuDemands>();
			builder.AddNotifications();
			builder.AddSidePane();

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
			await host.UseTaskbar(host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>().Value.ConfigurationFilePath);
			await host.UseSystemTray();
			await host.UseStartMenu();
			await host.UseNotifications();
			await host.UseSidePane();
			await host.UseGlimpseHost();

			await store.Dispatch(new InitializeStoreAction());

			var orchestrator = host.Services.GetRequiredService<DisplayOrchestrator>();
			orchestrator.WatchMonitorChanges();

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
