using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
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

	private readonly IObservable<DateTime> _oneSecondTimer;
	private readonly Subject<Unit> _clockClicked = new();
	private readonly Box _panelRoot;
	private readonly Box _centerBox;

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

		_panelRoot = new Box(Orientation.Horizontal, 0);
		_panelRoot.AddClass("panel");

		_centerBox = new Box(Orientation.Horizontal, 0);
		_centerBox.Add(startMenuIcon.Widget);
		_centerBox.Add(taskbarView.Widget);
		_centerBox.Valign = Align.Center;
		_centerBox.Halign = Align.Start;
		_centerBox.Expand = true;
		_centerBox.Spacing = 4;
		_panelRoot.Add(_centerBox);

		var rightBox = new Box(Orientation.Horizontal, 0);
		rightBox.PackStart(systemTrayBox, false, false, 4);
		rightBox.PackStart(CreateClock(), false, false, 5);
		rightBox.Halign = Align.End;
		rightBox.Valign = Align.Center;
		_panelRoot.Add(rightBox);

		var panelRootEventBox = new EventBox();
		panelRootEventBox.Add(_panelRoot);
		panelRootEventBox.ShowAll();

		taskbarView.Widget.ObserveEvent(e => e.Events().SizeAllocated)
			.ObserveOn(GLibExt.Scheduler)
			.Subscribe(_ => _centerBox.MarginStart = ComputeCenterBoxMarginLeft());

		store.Select(TaskbarViewModelSelectors.CurrentSlots)
			.DistinctUntilChanged()
			.TakeUntilDestroyed(_panelRoot)
			.ObserveOn(GLibExt.Scheduler)
			.Select(g => g.Refs.Count)
			.DistinctUntilChanged()
			.Subscribe(_ => _centerBox.MarginStart = ComputeCenterBoxMarginLeft());

		var taskManagerObs = store
			.Select(TaskbarSelectors.TaskManagerCommand)
			.TakeUntilDestroyed(_panelRoot)
			.ObserveOn(GLibExt.Scheduler);

		var contextMenuViewModel = Observable
			.Return(ImmutableList<ContextMenuItemViewModel>.Empty
				.Add(new ContextMenuItemViewModel()
				{
					DisplayText = "Task Manager",
					Icon = new ImageViewModel() { IconNameOrPath = "utilities-system-monitor" }
				}));

		var menu = ContextMenuFactory.Create(panelRootEventBox, contextMenuViewModel);
		menu.ItemActivated.WithLatestFrom(taskManagerObs).Subscribe(t => DesktopFileRunner.Run(t.Second));
		panelRootEventBox.Events().Destroyed.ObserveOn(GLibExt.Scheduler).Take(1).Subscribe(_ => menu.Destroy());

		Window = new Window(WindowType.Toplevel);
		Window.Decorated = false;
		Window.Resizable = false;
		Window.TypeHint = WindowTypeHint.Dock;
		Window.AppPaintable = true;
		Window.Visual = Window.Screen.RgbaVisual;

		Window.ObserveEvent<Widget, ButtonReleaseEventArgs>(w => w.Events().ButtonReleaseEvent).Subscribe(_ => Window.Window.Focus(0));
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
		return _panelRoot.Allocation.Width / 2 - _centerBox.Allocation.Width / 2;
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
			.ObserveOn(GLibExt.Scheduler)
			.Select(dt => dt.ToString(ClockFormat))
			.DistinctUntilChanged()
			.Subscribe(t => clockLabel.Text = t);

		clockButtonEventBox.ObserveEvent<Widget, ButtonReleaseEventArgs>(w => w.Events().ButtonReleaseEvent).Where(e => e.Event.Button == 1).Subscribe(e =>
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
