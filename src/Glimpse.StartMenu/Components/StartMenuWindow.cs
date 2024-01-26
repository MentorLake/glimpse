using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
using GLib;
using Glimpse.Common.System.Reactive;
using Glimpse.Freedesktop.DesktopEntries;
using Glimpse.Interop.Gdk;
using Glimpse.UI;
using Glimpse.Xorg.State;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;
using Key = Gdk.Key;
using WindowType = Gtk.WindowType;

namespace Glimpse.StartMenu.Components;

public class StartMenuWindow : Gtk.Window
{
	private readonly Subject<EventConfigure> _configureEventSubject = new();
	private readonly StartMenuContent _startMenuContent;
	private readonly Revealer _revealer;

	public IObservable<Point> WindowMoved { get; }

	public StartMenuWindow(ReduxStore store, IStartMenuDemands startMenuDemands, StartMenuSelectors startMenuSelectors)
		: base(WindowType.Toplevel)
	{
		SkipPagerHint = true;
		SkipTaskbarHint = true;
		Decorated = false;
		Resizable = false;
		CanFocus = false;
		TypeHint = WindowTypeHint.Dialog;
		Visual = Screen.RgbaVisual;
		AppPaintable = true;
		Visible = false;
		KeepAbove = true;

		this.Events().DeleteEvent.Subscribe(e => e.RetVal = true);

		WindowMoved = _configureEventSubject
			.TakeUntilDestroyed(this)
			.Select(e => new Point(e.X, e.Y))
			.DistinctUntilChanged((a, b) => a.X == b.X && a.Y == b.Y);

		var viewModelObservable = store.Select(startMenuSelectors.ViewModel)
			.TakeUntilDestroyed(this)
			.ObserveOn(new SynchronizationContextScheduler(new GLibSynchronizationContext(), false))
			.Replay(1);

		var actionBar = new StartMenuActionBar(viewModelObservable.Select(v => v.ActionBarViewModel).DistinctUntilChanged());

		this.ObserveEvent(actionBar.CommandInvoked).Subscribe(command =>
		{
			ToggleVisibility();
			DesktopFileRunner.Run(command);
		});

		_startMenuContent = new StartMenuContent(viewModelObservable, actionBar);

		this.ObserveEvent(_startMenuContent.DesktopFileAction).Subscribe(a => DesktopFileRunner.Run(a));
		this.ObserveEvent(_startMenuContent.ChipActivated).Subscribe(c => store.Dispatch(new UpdateAppFilteringChip(c)));
		this.ObserveEvent(_startMenuContent.AppOrderingChanged).Subscribe(t => store.Dispatch(new UpdateStartMenuPinnedAppOrderingAction(t)));
		this.ObserveEvent(_startMenuContent.ToggleStartMenuPinning).Subscribe(f => store.Dispatch(new ToggleStartMenuPinningAction(f)));
		this.ObserveEvent(_startMenuContent.ToggleTaskbarPinning).Subscribe(startMenuDemands.ToggleDesktopFilePinning);
		this.ObserveEvent(_startMenuContent.SearchTextUpdated).Subscribe(text => store.Dispatch(new UpdateStartMenuSearchTextAction(text)));
		this.ObserveEvent(_startMenuContent.AppLaunch).Subscribe(desktopFile =>
		{
			ToggleVisibility();
			DesktopFileRunner.Run(desktopFile);
		});

		store.Actions
			.OfType<WindowFocusedChangedAction>()
			.ObserveOn(new GLibSynchronizationContext())
			.TakeUntilDestroyed(this)
			.Where(action => IsVisible && action.WindowRef.Id != LibGdk3Interop.gdk_x11_window_get_xid(Window.Handle))
			.Subscribe(_ => ToggleVisibility());

		store.Actions.OfType<StartMenuOpenedAction>()
			.ObserveOn(new GLibSynchronizationContext())
			.TakeUntilDestroyed(this)
			.Subscribe(_ => ToggleVisibility());

		_revealer = new Revealer();
		_revealer.AddMany(_startMenuContent);
		_revealer.TransitionDuration = 250;
		_revealer.TransitionType = RevealerTransitionType.SlideUp;
		_revealer.Show();
		_revealer.Valign = Align.End;

		SetSizeRequest(640, 725);

		Add(_revealer);
		viewModelObservable.Connect();
	}

	public void ToggleVisibility()
	{
		Display.GetPointer(out var x, out var y);
		var eventMonitor = Display.GetMonitorAtPoint(x, y);

		if (IsVisible)
		{
			_revealer.RevealChild = false;
			Observable.Timer(TimeSpan.FromMilliseconds(250)).ObserveOn(new GLibSynchronizationContext()).Subscribe(_ => Hide());
		}
		else
		{
			Show();
			this.CenterOnScreenAtBottom(eventMonitor);
			_revealer.RevealChild = true;
		}
	}

	protected override bool OnConfigureEvent(EventConfigure evnt)
	{
		_configureEventSubject.OnNext(evnt);
		return base.OnConfigureEvent(evnt);
	}

	protected override void OnShown()
	{
		base.OnShown();
		_startMenuContent.HandleWindowShown();
	}

	[ConnectBefore]
	protected override bool OnKeyPressEvent(EventKey evnt)
	{
		if (evnt.Key == Key.Escape)
		{
			ToggleVisibility();
			return true;
		}

		if (_startMenuContent.HandleKeyPress(evnt.KeyValue))
		{
			return true;
		}

		return base.OnKeyPressEvent(evnt);
	}
}
