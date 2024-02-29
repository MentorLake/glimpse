using System.CommandLine;
using System.Reactive;
using System.Reactive.Linq;
using GLib;
using Glimpse.Common.Accounts;
using Glimpse.Common.Configuration;
using Glimpse.Common.DBus;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.StatusNotifierWatcher;
using Glimpse.Common.System.Reactive;
using Glimpse.Common.Xorg;
using Glimpse.Common.XSessionManagement;
using Glimpse.Notifications;
using Glimpse.SidePane.Components.Calendar;
using Glimpse.SidePane.Components.SidePane;
using Glimpse.StartMenu;
using Glimpse.SystemTray;
using Glimpse.Taskbar;
using Glimpse.Taskbar.Components.Panel;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application = Gtk.Application;

namespace Glimpse;

public static class Program
{
	public static async Task<int> Main(string[] args)
	{
		var installCommand = new Command("install", "Install Glimpse");
		installCommand.AddAlias("i");
		installCommand.SetHandler(async _ =>
		{
			Installation.RunScript(Installation.InstallScriptResourceName);

			var builder = Host.CreateApplicationBuilder(Array.Empty<string>());
			builder.AddDBus();
			builder.AddXSessionManagement("org_glimpse");
			var host = builder.Build();

			var connections = host.Services.GetRequiredService<DBusConnections>();
			await connections.System.ConnectAsync();
			await connections.Session.ConnectAsync();

			var xSessionManager = host.Services.GetRequiredService<XSessionManager>();
			await xSessionManager.Register(Installation.DefaultInstallPath);
		});

		var uninstallCommand = new Command("uninstall", "Uninstall Glimpse");
		uninstallCommand.AddAlias("u");
		uninstallCommand.SetHandler(_ => Installation.RunScript(Installation.UninstallScriptResourceName));

		var rootCommand = new RootCommand("Glimpse");
		rootCommand.AddCommand(installCommand);
		rootCommand.AddCommand(uninstallCommand);
		rootCommand.SetHandler(async c => c.ExitCode = await RunGlimpseAsync());

		return await rootCommand.InvokeAsync(args);
	}

	private static async Task<int> RunGlimpseAsync()
	{
		try
		{
			AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => Console.WriteLine(eventArgs.ExceptionObject);
			var builder = Host.CreateApplicationBuilder(Array.Empty<string>());
			builder.Services.AddSingleton<ReduxStore>();
			builder.AddXorg();
			builder.AddDesktopFiles();
			builder.AddDBus();
			builder.AddStatusNotifier();
			builder.AddXSessionManagement("org_glimpse");
			builder.AddAccounts();
			builder.AddGlimpseConfiguration();
			builder.AddXorg();
			builder.Services.AddHostedService<GlimpseGtkApplication>();
			builder.Services.AddTransient<Panel>();
			builder.Services.AddSingleton<GlimpseGtkApplication>();
			builder.Services.AddSingleton<IStartMenuDemands, StartMenuDemands>();
			builder.Services.AddSingleton<CalendarWindow>();
			builder.Services.AddSingleton<SidePaneWindow>();

			builder.Services.AddKeyedSingleton(Timers.OneSecond, (c, _) =>
			{
				var host1 = c.GetRequiredService<IHostApplicationLifetime>();
				var shuttingDown = Observable.Create<Unit>(obs => host1.ApplicationStopping.Register(() => obs.OnNext(Unit.Default)));
				return TimerFactory.OneSecondTimer.TakeUntil(shuttingDown).Publish().AutoConnect();
			});

			builder.Services.AddSingleton(_ =>
			{
				var app = new Application("org.glimpse", ApplicationFlags.None);
				app.AddAction(new SimpleAction("OpenStartMenu", null));
				app.AddAction(new SimpleAction("LoadPanels", null));
				return app;
			});

			builder.AddTaskbar();
			builder.AddSystemTray();
			builder.AddStartMenu();
			builder.AddNotifications();

			var host = builder.Build();

			var store = host.Services.GetRequiredService<ReduxStore>();
			store.RegisterReducers(host.Services.GetServices<IReducerFactory>().ToArray());
			store.RegisterReducers(host.Services.GetServices<FeatureReducerCollection>().ToArray());
			store.RegisterEffects(host.Services.GetServices<IEffectsFactory>().ToArray()
				.SelectMany(e => e.Create())
				.Select(oldEffect => new Effect()
				{
					Run = _ => oldEffect.Run(store.Actions).Do(_ => { }, exception => Console.WriteLine(exception)),
					Config = oldEffect.Config
				})
				.ToArray());

			await host.UseDBus();
			await host.UseDesktopFiles();
			await host.UseXorg();
			await host.UseGlimpseConfiguration();
			await host.UseXSessionManagement(Installation.DefaultInstallPath);
			await host.UseAccounts();
			await host.UseStatusNotifier();
			await host.UseTaskbar();
			await host.UseSystemTray();
			await host.UseStartMenu();
			await host.UseNotifications();

			await store.Dispatch(new InitializeStoreAction());
			await host.RunAsync();
			return 0;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return 1;
		}
	}
}
