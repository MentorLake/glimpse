using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
using GLib;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Glimpse.Common.System.Reactive;
using Glimpse.SidePane.Components.SidePane;
using Glimpse.SystemTray.Components;
using Glimpse.Taskbar.Components.ApplicationIcons;
using Gtk;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using ReactiveMarbles.ObservableEvents;
using DateTime = System.DateTime;
using Unit = System.Reactive.Unit;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace Glimpse.Taskbar.Components.Panel;

public class Panel
{
	public StartMenuIcon.StartMenuIcon StartMenuIcon { get; }
	public Window Window { get; }
	private const string ClockFormat = "h:mm tt\nM/d/yyyy";

	private readonly TaskbarView _taskbarView;
	private readonly IObservable<DateTime> _oneSecondTimer;
	private readonly Subject<Unit> _clockClicked = new();
	private readonly Box _panelRoot;

	public Panel(
		SystemTrayBox systemTrayBox,
		TaskbarView taskbarView,
		StartMenuIcon.StartMenuIcon startMenuIcon,
		ReduxStore store,
		SidePaneWindow sidePaneWindow,
		[FromKeyedServices(Timers.OneSecond)] IObservable<DateTime> oneSecondTimer)
	{
		StartMenuIcon = startMenuIcon;
		_taskbarView = taskbarView;
		_taskbarView.Valign = Align.Center;
		_oneSecondTimer = oneSecondTimer;

		_panelRoot = new Box(Orientation.Horizontal, 0);
		_panelRoot.AddClass("panel");

		var centerBox = new Box(Orientation.Horizontal, 0);
		centerBox.PackStart(startMenuIcon.Widget, false, false, 0);
		centerBox.PackStart(taskbarView, false, false, 0);
		centerBox.Halign = Align.Start;
		_panelRoot.Add(centerBox);

		var rightBox = new Box(Orientation.Horizontal, 0);
		rightBox.PackStart(systemTrayBox, false, false, 4);
		rightBox.PackStart(CreateClock(), false, false, 5);
		rightBox.Halign = Align.End;
		rightBox.Valign = Align.Center;
		rightBox.Hexpand = true;
		_panelRoot.Add(rightBox);

		var panelRootEventBox = new EventBox();
		panelRootEventBox.Add(_panelRoot);
		panelRootEventBox.ShowAll();

		_panelRoot.ObserveEvent(e => e.Events().SizeAllocated)
			.Subscribe(_ => centerBox.MarginStart = ComputeCenterBoxMarginLeft());

		store.Select(TaskbarViewModelSelectors.CurrentSlots)
			.DistinctUntilChanged()
			.TakeUntilDestroyed(_panelRoot)
			.ObserveOn(new SynchronizationContextScheduler(new GLibSynchronizationContext(), false))
			.Select(g => g.Refs.Count)
			.DistinctUntilChanged()
			.Subscribe(_ => centerBox.MarginStart = ComputeCenterBoxMarginLeft());

		var taskManagerObs = store
			.Select(TaskbarSelectors.TaskManagerCommand)
			.TakeUntilDestroyed(_panelRoot)
			.ObserveOn(new SynchronizationContextScheduler(new GLibSynchronizationContext(), false));

		var contextMenuViewModel = Observable
			.Return(ImmutableList<ContextMenuItemViewModel>.Empty
				.Add(new ContextMenuItemViewModel()
				{
					DisplayText = "Task Manager",
					Icon = new ImageViewModel() { IconNameOrPath = "utilities-system-monitor" }
				}));

		var menu = ContextMenuFactory.Create(panelRootEventBox, contextMenuViewModel);
		menu.ItemActivated.WithLatestFrom(taskManagerObs).Subscribe(t => DesktopFileRunner.Run(t.Second));
		panelRootEventBox.Events().Destroyed.Take(1).Subscribe(_ => menu.Destroy());

		Window = new Window(WindowType.Toplevel);
		Window.Decorated = false;
		Window.Resizable = false;
		Window.TypeHint = WindowTypeHint.Dock;
		Window.AppPaintable = true;
		Window.Visual = Window.Screen.RgbaVisual;

		Window.ObserveButtonRelease().Subscribe(_ => Window.Window.Focus(0));
		Window.Events().DeleteEvent.TakeUntilDestroyed(Window).Subscribe(e => e.RetVal = true);

		_clockClicked.TakeUntilDestroyed(Window).Subscribe(_ =>
		{
			Window.Display.GetPointer(out var x, out var y);
			var eventMonitor = Window.Display.GetMonitorAtPoint(x, y);
			sidePaneWindow.ToggleVisibility(eventMonitor.Geometry.Right, eventMonitor.Geometry.Bottom - Window.AllocatedHeight);
		});

		Window.Add(panelRootEventBox);
		Window.ShowAll();
	}

	private int ComputeCenterBoxMarginLeft()
	{
		return _panelRoot.Allocation.Width / 2 - _taskbarView.Allocation.Width / 2;
	}

	private Widget CreateClock()
	{
		var notificationImage = new Image();
		notificationImage.IconName = "notification-symbolic";
		notificationImage.PixelSize = 16;

		var clockLabel = new Label(DateTime.Now.ToString(ClockFormat));
		clockLabel.Justify = Justification.Right;

		var clockButton = new Box(Orientation.Horizontal, 0);
		clockButton.AddClass("clock");
		clockButton.Halign = Align.Center;
		clockButton.AddMany(clockLabel, notificationImage);

		var clockButtonEventBox = new EventBox();
		clockButtonEventBox.AddClass("button");
		clockButtonEventBox.AddButtonStates();
		clockButtonEventBox.Add(clockButton);

		_oneSecondTimer
			.TakeUntilDestroyed(_panelRoot)
			.ObserveOn(new GLibSynchronizationContext())
			.Select(dt => dt.ToString(ClockFormat))
			.DistinctUntilChanged()
			.Subscribe(t => clockLabel.Text = t);

		clockButtonEventBox.ObserveButtonRelease().Where(e => e.Event.Button == 1).Subscribe(e =>
		{
			_clockClicked.OnNext(Unit.Default);
			e.RetVal = true;
		});

		return clockButtonEventBox;
	}

	public bool IsOnMonitor(Rectangle monitor)
	{
		Window.GetPosition(out var x, out var y);
		return monitor.Contains(x, y);
	}
}
