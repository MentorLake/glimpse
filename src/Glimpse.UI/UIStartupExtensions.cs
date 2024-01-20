using System.Reactive.Linq;
using GLib;
using Glimpse.Common.System.Reactive;
using Glimpse.Freedesktop.Notifications;
using Glimpse.StartMenu;
using Glimpse.Taskbar;
using Glimpse.UI.Components;
using Glimpse.UI.Components.SidePane;
using Glimpse.UI.Components.SidePane.Calendar;
using Glimpse.UI.Components.SystemTray;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;
using Unit = System.Reactive.Unit;

namespace Glimpse.UI;

public static class UIStartupExtensions
{
	public static async Task UseGlimpseUI(this IHost host)
	{
		await host.UseTaskbar();
		await host.UseSystemTray();
		await host.UseStartMenu();
		await host.UseNotifications();
	}

	public static void AddGlimpseUI(this IHostApplicationBuilder builder)
	{
		builder.Services.AddHostedService<GlimpseGtkApplication>();
		builder.Services.AddTransient<Panel>();
		builder.Services.AddSingleton<GlimpseGtkApplication>();
		builder.Services.AddSingleton<IStartMenuDemands, StartMenuDemands>();
		builder.Services.AddSingleton<CalendarWindow>();
		builder.Services.AddSingleton<SidePaneWindow>();

		builder.Services.AddKeyedSingleton(Timers.OneSecond, (c, _) =>
		{
			var host = c.GetRequiredService<IHostApplicationLifetime>();
			var shuttingDown = Observable.Create<Unit>(obs => host.ApplicationStopping.Register(() => obs.OnNext(Unit.Default)));
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
	}
}
