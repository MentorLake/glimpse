using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using Gdk;
using GLib;
using Glimpse.Common.Configuration;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Xorg;
using Glimpse.StartMenu;
using Glimpse.StartMenu.Components;
using Glimpse.Taskbar;
using Gtk;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;
using Window = Gtk.Window;

namespace Glimpse.Tests;

public class Program
{
	public static async Task Main(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.AddEnvironmentVariables()
			.AddJsonStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("appsettings.json")!)
			.Build();

		var startMenuContentViewModel = new StartMenuViewModel()
		{
			Chips = ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip>.Empty
				.Add(StartMenuChips.Pinned, new StartMenuAppFilteringChip() { IsSelected = true, IsVisible = true })
				.Add(StartMenuChips.AllApps, new StartMenuAppFilteringChip() { IsSelected = false, IsVisible = true })
				.Add(StartMenuChips.SearchResults, new StartMenuAppFilteringChip() { IsSelected = false, IsVisible = true }),
			AllApps = Enumerable.Range(1, 45).Select(i => CreateStartMenuAppViewModel(i.ToString())).ToImmutableList(),
		};

		var appName = configuration.GetValue<string>("Glimpse:ApplicationName");
		using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddConsole().AddJournal(o => o.SyslogIdentifier = appName));
		var bootstrapLogger = bootstrapLoggerFactory.CreateLogger(appName);

		var builder = Host.CreateApplicationBuilder([]);
		builder.Configuration.AddConfiguration(configuration);
		builder.Services.AddSingleton<ReduxStore>();
		builder.AddTaskbar();
		builder.AddStartMenu<StartMenuDemands>();
		builder.AddGlimpseConfiguration();
		builder.AddXorg();
		builder.AddDesktopFiles();

		var host = builder.Build();
		await host.UseXorg();
		await host.UseDesktopFiles();
		await host.UseGlimpseConfiguration();
		await host.UseTaskbar(host.Services.GetRequiredService<IOptions<GlimpseAppSettings>>().Value.ConfigurationFilePath);
		await host.UseStartMenu();

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

		var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

		lifetime.ApplicationStarted.Register(() =>
		{
			var application = new Application("org.glimpsetests", ApplicationFlags.None);
			Application.Init();
			application.Register(Cancellable.Current);
			LoadCss();

			var window = new Window("Test");
			//window.Add(host.Services.GetRequiredService<TaskbarView>());
			window.Add(new StartMenuContent(new BehaviorSubject<StartMenuViewModel>(startMenuContentViewModel), new StartMenuActionBar(Observable.Return(new ActionBarViewModel()))));
			application.AddWindow(window);
			window.ShowAll();
			Task.Run(Application.Run);
		});

		await host.RunAsync();
	}

	private static void LoadCss()
	{
		var allCss = new StringBuilder();

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (var cssFile in assembly.GetManifestResourceNames().Where(n => n.EndsWith(".css")))
			{
				using var cssFileStream = new StreamReader(assembly.GetManifestResourceStream(cssFile));
				allCss.AppendLine(cssFileStream.ReadToEnd());
			}
		}

		var display = Display.Default;
		var screen = display.DefaultScreen;
		var screenCss = new CssProvider();
		screenCss.LoadFromData(allCss.ToString());
		StyleContext.AddProviderForScreen(screen, screenCss, uint.MaxValue);
	}

	private static StartMenuAppViewModel CreateStartMenuAppViewModel(string id)
	{
		var iconIndex = Random.Shared.Next() % 2;
		return new StartMenuAppViewModel()
		{
			Index = 2,
			IsVisible = true,
			Icon = new() { IconNameOrPath = iconIndex == 0 ? "code" : "spotify" },
			IsPinnedToStartMenu = true,
			DesktopFile = new() { Name = id, Id = id },
		};
	}
}
