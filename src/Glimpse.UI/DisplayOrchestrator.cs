using System.Collections.Immutable;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Services.StartMenu;
using Glimpse.UI.Components.NotificationBubbles;
using Glimpse.UI.Components.Panel;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.SidePane;
using Glimpse.UI.Components.StartMenu;
using MentorLake.Gdk;
using MentorLake.GdkPixbuf;
using MentorLake.Gio;
using MentorLake.Gtk;
using MentorLake.Gtk3;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Glimpse.UI;

public class DisplayOrchestrator(NotificationBubblesService notificationBubblesService, GtkApplicationHandle application, IServiceProvider serviceProvider, ReduxStore store, ILogger<DisplayOrchestrator> logger)
{
	private readonly List<Panel> _panels = new();
	private StartMenuWindow _startMenuWindow;

	public void Init()
	{
		application.Signal_Startup().Take(1).SubscribeDebug(_ =>
		{
			_startMenuWindow = serviceProvider.GetRequiredService<StartMenuWindow>();
			application.AddWindow(_startMenuWindow.Window);
			application.AddWindow(serviceProvider.GetRequiredService<SidePaneWindow>().Window);

			var iconManager = IconManager.GetDefault();

			store.Select(XorgSelectors.Windows).ObserveOn(GLibExt.Scheduler).Select(w => w.ById).UnbundleMany(w => w.Key).RemoveIndex().SubscribeDebug(windowObs =>
			{
				windowObs.SubscribeDebug(props =>
				{
					var icon = props.Value.Icons.MaxBy(i => i.GetWidth());
					if (icon != null) iconManager.AddKeyedIcon(props.Key.ToString(), icon);
				});
				windowObs.TakeLast(1).Subscribe(props => iconManager.RemoveKeyedIcon(props.Key.ToString()));
			});
		});

		var openStartMenuAction = GSimpleActionHandle.New("OpenStartMenu", null);
		openStartMenuAction.Signal_Activate().Select(_ => true).SubscribeDebug(_ => store.Dispatch(new StartMenuOpenedAction()));
		application.AddAction(openStartMenuAction);

		var loadPanels = GSimpleActionHandle.New("LoadPanels", null);
		loadPanels.Signal_Activate().Subscribe(_ =>
		{
			store.Dispatch(new UpdateMonitorsAction([]));
			store.Dispatch(new UpdateMonitorsAction(GdkDisplayHandle.GetDefault().GetMonitors()));
		});
		application.AddAction(loadPanels);
	}

	private void StackNotificationsOnMonitor(GdkMonitorHandle monitor, int panelHeight, ImmutableList<NotificationBubbleWindow> notificationWindows)
	{
		var monitorGeometry = monitor.GetGeometryRect();
		var monitorWorkArea = monitor.GetWorkAreaRect();
		var currentTopOfWidgetBelowNotification = monitorGeometry.Height - panelHeight;

		for (var i = 0; i < notificationWindows.Count; i++)
		{
			var window = notificationWindows[i];
			var windowLeft = monitorWorkArea.Right - window.Window.GetAllocatedWidth() - 8;
			var windowTop = currentTopOfWidgetBelowNotification - window.Window.GetAllocatedHeight() - 8;
			window.Window.Move(windowLeft, windowTop);
			currentTopOfWidgetBelowNotification = windowTop;
		}
	}

	public Panel CreatePanel(GdkMonitorHandle monitor, int bottomOffset = 0)
	{
		var panelWindow = serviceProvider.GetRequiredService<Panel>();
		_panels.Add(panelWindow);
		panelWindow.Widget.Signal_Destroy().Take(1).SubscribeDebug(_ => _panels.Remove(panelWindow));
		panelWindow.Widget.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).Where(_ => _startMenuWindow.Window.IsVisible()).SubscribeDebug(w => _startMenuWindow.ToggleVisibility());
		var monitorGeometry = monitor.GetGeometryRect();
		var monitorWorkArea = monitor.GetWorkAreaRect();
		DockToBottom(panelWindow.Widget, new Rectangle(monitorGeometry.Location, new Size(monitorGeometry.Width, monitorGeometry.Bottom)), monitorWorkArea, bottomOffset);
		application.AddWindow(panelWindow.Widget);

		var startMenuIcon = panelWindow.StartMenuIcon;

		_startMenuWindow.Window
			.ObserveEvent(w => w.Signal_Hide())
			.SubscribeDebug(_ => startMenuIcon.StartMenuClosed());

		_startMenuWindow.Window
			.ObserveEvent(w => w.Signal_Show().Select(_ => true))
			.Merge(_startMenuWindow.WindowMoved.Select(_ => true))
			.TakeUntilDestroyed(startMenuIcon.Widget)
			.SubscribeDebug(_ => startMenuIcon.StartMenuOpened());

		startMenuIcon.StartMenuButtonClicked
			.TakeUntilDestroyed(startMenuIcon.Widget)
			.SubscribeDebug(_ => _startMenuWindow.ToggleVisibility());

		return panelWindow;
	}

	private void DockToBottom(GtkWindowHandle window, Rectangle monitorDimensions, Rectangle workArea, int bottomOffset)
	{
		var windowAllocation = window.GetAllocationRect();
		window.SetSizeRequest(monitorDimensions.Width, windowAllocation.Height);
		window.Move(workArea.Left, monitorDimensions.Bottom - windowAllocation.Height - bottomOffset);
		window.ReserveSpace(monitorDimensions, workArea);
	}

	public void WatchMonitorChanges()
	{
		application.Signal_Activate().Take(1).Subscribe(_ =>
		{
			var display = GdkDisplayHandle.GetDefault();
			var screen = display.GetDefaultScreen();

			store.Dispatch(new UpdateMonitorsAction(display.GetMonitors()));

			screen.Signal_SizeChanged().Select(_ => Unit.Default)
				.Merge(screen.Signal_MonitorsChanged().Select(_ => Unit.Default))
				.SubscribeDebug(_ => store.Dispatch(new UpdateMonitorsAction(display.GetMonitors())));

			store
				.Select(GlimpseGtkSelectors.Monitors)
				.ObserveOn(GLibExt.Scheduler)
				.UnbundleMany(m => m.GetHashCode())
				.SubscribeDebug(obs =>
				{
					var newPanel = CreatePanel(obs.Key);

					obs.SkipLast(1).Subscribe(_ =>
					{
						DockToBottom(newPanel.Widget, obs.Key.GetGeometryRect(), obs.Key.GetWorkAreaRect(), 1);
					});

					obs.TakeLast(1).Subscribe(_ =>
					{
						GtkWindowHandleExtensions.Close(newPanel.Widget);
						newPanel.Widget.Dispose();
					});
				});
		});
	}

	public void WatchNotifications()
	{
		var notificationsPerMonitor = new Dictionary<long, ImmutableList<NotificationBubbleWindow>>();

		notificationBubblesService.Notifications.Subscribe(w =>
		{
			var display = GdkDisplayHandle.GetDefault();
			display.GetPointer(out _, out var x, out var y, out _);
			var eventMonitor = display.GetMonitorAtPoint(x, y);
			application.AddWindow(w.Window);

			var eventMonitorGeometry = eventMonitor.GetGeometryRect();
			var panel = _panels.FirstOrDefault(p => p.IsOnMonitor(eventMonitorGeometry));

			if (panel == null)
			{
				logger.LogWarning("Couldn't find panel on monitor to display notification.  Defaulting to first panel.");
				panel = _panels.First();
				eventMonitor = display.GetMonitors().First(m => panel.IsOnMonitor(m.GetGeometryRect()));
			}

			panel.Widget.GetWindow().GetGeometry(out _, out _, out _, out var panelHeight);
			var monitorId = eventMonitor.GetManagedUniqueId();
			notificationsPerMonitor.TryAdd(monitorId, ImmutableList<NotificationBubbleWindow>.Empty);
			notificationsPerMonitor[monitorId] = notificationsPerMonitor[monitorId].Add(w);

			w.Window.Signal_SizeAllocate().TakeUntilDestroyed(w.Window).SubscribeDebug(_ =>
			{
				StackNotificationsOnMonitor(eventMonitor, panelHeight, notificationsPerMonitor[monitorId]);
			});

			w.Window.Signal_Unmap().Take(1).SubscribeDebug(_ =>
			{
				notificationsPerMonitor[monitorId] = notificationsPerMonitor[monitorId].Remove(w);
				StackNotificationsOnMonitor(eventMonitor, panelHeight, notificationsPerMonitor[monitorId]);
			});
		});
	}
}
