using System.CommandLine;
using System.Reactive.Linq;
using Glimpse.Configuration;
using Glimpse.Freedesktop;
using Glimpse.Freedesktop.DBus;
using Glimpse.Freedesktop.DesktopEntries;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Glimpse.UI;
using Glimpse.Xorg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
			builder.AddFreedesktop();
			builder.AddGlimpseConfiguration();
			builder.AddXorg();
			builder.AddGlimpseUI();

			var host = builder.Build();


			var store = host.Services.GetRequiredService<ReduxStore>();
			store.RegisterReducers(host.Services.GetServices<FeatureReducerCollection>().ToArray());
			store.RegisterEffects(host.Services.GetServices<IEffectsFactory>().ToArray()
				.SelectMany(e => e.Create())
				.Select(oldEffect => new Effect()
				{
					Run = _ => oldEffect.Run(store.Actions).Do(_ => { }, exception => Console.WriteLine(exception)),
					Config = oldEffect.Config
				})
				.ToArray());

			await host.UseDesktopFiles();
			await host.UseXorg();
			await host.UseGlimpseConfiguration();
			await host.UseFreedesktop(Installation.DefaultInstallPath);
			await host.UseGlimpseUI();

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
