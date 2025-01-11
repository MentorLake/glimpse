using System.Collections.Immutable;
using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Services.Taskbar;
using Glimpse.UI.Components.ApplicationIcons;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ContextMenu;
using Glimpse.UI.Components.SidePane;
using Glimpse.UI.Components.SystemTray;
using MentorLake.Gdk;
using MentorLake.Gtk;
using MentorLake.Gtk3;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using DateTime = System.DateTime;
using Unit = System.Reactive.Unit;

namespace Glimpse.UI.Components.Panel;

public class Panel
{
	public StartMenuIcon.StartMenuIcon StartMenuIcon { get; }
	public GtkWindowHandle Widget { get; }
	private const string ClockFormat = "h:mm tt\nM/d/yyyy";

	private readonly IObservable<DateTime> _oneSecondTimer;
	private readonly Subject<Unit> _clockClicked = new();

	public Panel(
		SystemTrayBox systemTrayBox,
		TaskbarView taskbarView,
		StartMenuIcon.StartMenuIcon startMenuIcon,
		ReduxStore store,
		SidePaneWindow sidePaneWindow,
		[FromKeyedServices(Timers.OneSecond)] IObservable<DateTime> oneSecondTimer)
	{
		StartMenuIcon = startMenuIcon;
		_oneSecondTimer = oneSecondTimer;

		var root = GtkOverlayHandle.New();

		var centerBox = GtkFixedHandle.New()
			.SetHalign(GtkAlign.GTK_ALIGN_CENTER)
			.SetValign(GtkAlign.GTK_ALIGN_CENTER)
			.Add(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 4)
				.Add(startMenuIcon.Widget)
				.Add(taskbarView.Widget));

		var rightBox = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
			.PackStart(systemTrayBox.Widget, false, false, 4)
			.PackStart(CreateClock(root), false, false, 5)
			.SetValign(GtkAlign.GTK_ALIGN_CENTER);

		var panel = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
			.AddClass("panel")
			.PackEnd(rightBox, false, false, 0);

		root.Add(panel).AddOverlay(centerBox);

		var taskManagerObs = store
			.Select(TaskbarSelectors.TaskManagerCommand)
			.TakeUntilDestroyed(root)
			.ObserveOn(GLibExt.Scheduler);

		var contextMenuViewModel = Observable
			.Return(ImmutableList<ContextMenuItemViewModel>.Empty
				.Add(new ContextMenuItemViewModel()
				{
					DisplayText = "Task Manager",
					Icon = new ImageViewModel() { IconNameOrPath = "utilities-system-monitor" }
				}));

		var panelRootEventBox = GtkEventBoxHandle.New()
			.Add(root)
			.ShowAll();

		var menu = ContextMenuFactory.Create<ContextMenuItemViewModel>(panelRootEventBox);
		menu.ItemActivated.WithLatestFrom(taskManagerObs).SubscribeDebug(t => DesktopFileRunner.Run(t.Second));
		contextMenuViewModel.DistinctUntilChanged().Subscribe(items => menu.UpdateItems(items));

		panelRootEventBox.Signal_Destroy().ObserveOn(GLibExt.Scheduler).Take(1).SubscribeDebug(_ => menu.Widget.Destroy());

		var window = GtkWindowHandle.New(GtkWindowType.GTK_WINDOW_TOPLEVEL)
			.SetDecorated(false)
			.SetResizable(false)
			.SetTypeHint(GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_DOCK)
			.With(w => w.SetVisual(w.GetScreen().GetRgbaVisual()))
			.AddClass("transparent")
			.With(w => w.ObserveEvent(x => x.Signal_ButtonReleaseEvent()).SubscribeDebug(_ => w.GetWindow().Focus(0)))
			.With(w => w.Signal_DeleteEvent().TakeUntilDestroyed(w).SubscribeDebug(e => e.ReturnValue = true))
			.Add(panelRootEventBox)
			.ShowAll();

		_clockClicked.TakeUntilDestroyed(window).SubscribeDebug(_ =>
		{
			window.GetDisplay().GetPointer(out var _, out var x, out var y, out var _);
			var eventMonitor = window.GetDisplay().GetMonitorAtPoint(x, y);
			var monitorGeometry = eventMonitor.GetGeometryRect();
			sidePaneWindow.ToggleVisibility(monitorGeometry.Right, monitorGeometry.Bottom - window.GetAllocatedHeight());
		});

		Widget = window;
	}

	private GtkWidgetHandle CreateClock(GtkWidgetHandle parent)
	{
		var notificationImage = GtkImageHandle.New();
		notificationImage.SetFromIconName("notification-symbolic", GtkIconSize.GTK_ICON_SIZE_BUTTON);
		notificationImage.SetPixelSize(16);

		var clockLabel = GtkLabelHandle.New(DateTime.Now.ToString(ClockFormat));
		clockLabel.SetJustify(GtkJustification.GTK_JUSTIFY_RIGHT);

		var clockButton = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		clockButton.AddClass("clock");
		clockButton.SetHalign(GtkAlign.GTK_ALIGN_CENTER);
		clockButton.AddMany(clockLabel, notificationImage);

		var clockButtonEventBox = GtkEventBoxHandle.New();
		clockButtonEventBox.AddClass("button");
		clockButtonEventBox.AddButtonStates();
		clockButtonEventBox.Add(clockButton);

		_oneSecondTimer
			.TakeUntilDestroyed(parent)
			.ObserveOn(GLibExt.Scheduler)
			.Select(dt => dt.ToString(ClockFormat))
			.DistinctUntilChanged()
			.SubscribeDebug(t => clockLabel.SetText(t));

		clockButtonEventBox.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).Where(e => e.Event.Dereference().button == 1).SubscribeDebug(e =>
		{
			_clockClicked.OnNext(Unit.Default);
			e.ReturnValue = true;
		});

		return clockButtonEventBox;
	}

	public bool IsOnMonitor(Rectangle monitor)
	{
		Widget.GetPosition(out var x, out var y);
		return monitor.Contains(x, y);
	}
}
