using System.Collections.Immutable;
using System.Reactive.Linq;
using Gdk;
using GLib;
using Glimpse.Common.Gtk;
using Glimpse.Common.System.Reactive;
using Glimpse.Notifications;
using Glimpse.Notifications.Components.NotificationBubbles;
using Glimpse.SidePane.Components.SidePane;
using Glimpse.StartMenu;
using Glimpse.StartMenu.Components;
using Glimpse.Taskbar.Components.Panel;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.ObservableEvents;
using Application = Gtk.Application;
using Monitor = Gdk.Monitor;

namespace Glimpse.Host;

public class DisplayOrchestrator(NotificationsService notificationsService, Application application, IServiceProvider serviceProvider, ReduxStore store)
{
	private readonly List<Panel> _panels = new();
	private StartMenuWindow _startMenuWindow;

	public void Init()
	{
		application.Events().Startup.ObserveOn(GLibExt.SynchronizationContext).Take(1).Subscribe(_ =>
		{
			_startMenuWindow = serviceProvider.GetRequiredService<StartMenuWindow>();
			application.AddWindow(_startMenuWindow);
			application.AddWindow(serviceProvider.GetRequiredService<SidePaneWindow>());
		});

		var openStartMenuAction = new SimpleAction("OpenStartMenu", null);
		openStartMenuAction.Events().Activated.Select(_ => true).Subscribe(_ => store.Dispatch(new StartMenuOpenedAction()));
		application.AddAction(openStartMenuAction);

		var loadPanels = new SimpleAction("LoadPanels", null);
		loadPanels.Events().Activated.Subscribe(_ =>
		{
			store.Dispatch(new UpdateMonitorsAction([]));
			store.Dispatch(new UpdateMonitorsAction(Display.Default.GetMonitors()));
		});
		application.AddAction(loadPanels);
	}

	private void StackNotificationsOnMonitor(Monitor monitor, int panelHeight, ImmutableList<NotificationBubbleWindow> notificationWindows)
	{
		var currentTopOfWidgetBelowNotification = monitor.Geometry.Height - panelHeight;

		for (var i = 0; i < notificationWindows.Count; i++)
		{
			var window = notificationWindows[i];
			var windowLeft = monitor.Workarea.Right - window.Allocation.Width - 8;
			var windowTop = currentTopOfWidgetBelowNotification - window.Allocation.Height - 8;
			window.Move(windowLeft, windowTop);
			currentTopOfWidgetBelowNotification = windowTop;
		}
	}

	public Panel CreatePanel(Monitor monitor, int bottomOffset = 1)
	{
		var panelWindow = serviceProvider.GetRequiredService<Panel>();
		_panels.Add(panelWindow);
		panelWindow.Window.Events().Destroyed.Take(1).Subscribe(_ => _panels.Remove(panelWindow));
		DockToBottom(panelWindow.Window, new Rectangle(monitor.Geometry.Location, new Size(monitor.Geometry.Width, monitor.Geometry.Bottom)), monitor.Workarea, bottomOffset);
		application.AddWindow(panelWindow.Window);

		var startMenuIcon = panelWindow.StartMenuIcon;

		_startMenuWindow.ObserveEvent(w => w.Events().Shown)
			.Merge(_startMenuWindow.ObserveEvent(w => w.Events().Hidden))
			.Merge(_startMenuWindow.WindowMoved.Select(x => (object) x))
			.TakeUntilDestroyed(startMenuIcon.Widget)
			.Subscribe(_ =>
			{
				if (!_startMenuWindow.IsVisible || startMenuIcon.Widget.Display.GetMonitorAtWindow(startMenuIcon.Widget.Window) != startMenuIcon.Widget.Display.GetMonitorAtWindow(_startMenuWindow.Window))
				{
					startMenuIcon.StartMenuOpened();
				}
				else
				{
					startMenuIcon.StartMenuClosed();
				}
			});


		startMenuIcon.StartMenuButtonClicked.TakeUntilDestroyed(startMenuIcon.Widget).Subscribe(location =>
		{
			_startMenuWindow.ToggleVisibility();
		});

		return panelWindow;
	}

	private void DockToBottom(Gtk.Window window, Rectangle monitorDimensions, Rectangle workArea, int bottomOffset)
	{
		window.SetSizeRequest(monitorDimensions.Width, window.AllocatedHeight);
		window.Move(workArea.Left, monitorDimensions.Bottom - window.AllocatedHeight - bottomOffset);
		ReserveSpace(window, monitorDimensions, workArea);
	}

	private void ReserveSpace(Gtk.Window window, Rectangle monitor, Rectangle workArea)
	{
		var reservedSpace = new long[] { 0, 0, 0, window.AllocatedHeight }.SelectMany(BitConverter.GetBytes).ToArray();
		Property.Change(window.Window, Atom.Intern("_NET_WM_STRUT", false), Atom.Intern("CARDINAL", false), 32, PropMode.Replace, reservedSpace, 4);

		var reservedSpaceLong = new long[] { 0, 0, 0, window.AllocatedHeight, 0, 0, 0, 0, 0, 0, workArea.Left, workArea.Left + monitor.Width - 1 }.SelectMany(BitConverter.GetBytes).ToArray();
		Property.Change(window.Window, Atom.Intern("_NET_WM_STRUT_PARTIAL", false), Atom.Intern("CARDINAL", false), 32, PropMode.Replace, reservedSpaceLong, 12);
	}

	public void WatchMonitorChanges()
	{
		application.Events().Startup.Subscribe(_ =>
		{
			var display = Display.Default;
			var screen = display.DefaultScreen;

			store.Dispatch(new UpdateMonitorsAction(display.GetMonitors()));

			screen.Events().SizeChanged
				.Merge(screen.Events().MonitorsChanged)
				.Subscribe(_ => store.Dispatch(new UpdateMonitorsAction(display.GetMonitors())));

			store
				.Select(GlimpseGtkSelectors.Monitors)
				.ObserveOn(GLibExt.Scheduler)
				.UnbundleMany(m => m.GetHashCode())
				.Subscribe(obs =>
				{
					var newPanel = CreatePanel(obs.Key);

					obs.SkipLast(1).Subscribe(_ =>
					{
						DockToBottom(newPanel.Window, obs.Key.Geometry, obs.Key.Workarea, 1);
					});

					obs.TakeLast(1).Subscribe(_ =>
					{
						newPanel.Window.Close();
						newPanel.Window.Dispose();
					});
				});
		});
	}

	public void WatchNotifications()
	{
		var notificationsPerMonitor = new Dictionary<Monitor, ImmutableList<NotificationBubbleWindow>>();

		notificationsService.Notifications.Subscribe(w =>
		{
			var display = Display.Default;
			display.GetPointer(out var x, out var y);
			var eventMonitor = display.GetMonitorAtPoint(x, y);
			application.AddWindow(w);

			var panel = _panels.FirstOrDefault(p => p.IsOnMonitor(eventMonitor.Geometry));
			panel.Window.Window.GetGeometry(out _, out _, out _, out var panelHeight);

			notificationsPerMonitor.TryAdd(eventMonitor, ImmutableList<NotificationBubbleWindow>.Empty);
			notificationsPerMonitor[eventMonitor] = notificationsPerMonitor[eventMonitor].Add(w);

			w.Events().SizeAllocated.TakeUntilDestroyed(w).Subscribe(_ =>
			{
				StackNotificationsOnMonitor(eventMonitor, panelHeight, notificationsPerMonitor[eventMonitor]);
			});

			w.Events().Unmapped.Take(1).Subscribe(_ =>
			{
				notificationsPerMonitor[eventMonitor] = notificationsPerMonitor[eventMonitor].Remove(w);
				StackNotificationsOnMonitor(eventMonitor, panelHeight, notificationsPerMonitor[eventMonitor]);
			});
		});
	}
}
