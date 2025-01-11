using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Services.StartMenu;
using Glimpse.Services.Taskbar;
using MentorLake.Gdk;
using MentorLake.GdkX11;
using MentorLake.GObject;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.UI.Components.StartMenu;

public class StartMenuWindow
{
	private readonly Subject<GdkEventConfigure> _configureEventSubject = new();
	private readonly StartMenuContent _startMenuContent;
	private readonly GtkRevealerHandle _revealer;
	private readonly GtkWindowHandle _root;

	public IObservable<Point> WindowMoved { get; }
	public GtkWindowHandle Window => _root;

	public StartMenuWindow(ReduxStore store, TaskbarService taskbarService)
	{
		_root = GtkWindowHandle.New(GtkWindowType.GTK_WINDOW_TOPLEVEL)
			.SetSkipPagerHint(true)
			.SetSkipTaskbarHint(true)
			.SetDecorated(false)
			.SetResizable(false)
			.SetCanFocus(false)
			.SetTypeHint(GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_DIALOG)
			.Prop(w => w.SetVisual(w.GetScreen().GetRgbaVisual()))
			.SetVisible(false)
			.SetKeepAbove(true)
			.AddClass("transparent")
			.ObserveEvent(w => w.Signal_DeleteEvent(), e => e.ReturnValue = true);

		WindowMoved = _configureEventSubject
			.TakeUntilDestroyed(_root)
			.Select(e => new Point(e.x, e.y))
			.DistinctUntilChanged((a, b) => a.X == b.X && a.Y == b.Y);

		var viewModelObservable = store
			.Select(StartMenuViewModelSelectors.ViewModel)
			.TakeUntilDestroyed(_root)
			.ObserveOn(GLibExt.Scheduler)
			.Replay(1);

		var actionBar = new StartMenuActionBar(viewModelObservable.Select(v => v.ActionBarViewModel).DistinctUntilChanged());

		_root.ObserveEvent(actionBar.CommandInvoked).SubscribeDebug(command =>
		{
			ToggleVisibility();
			DesktopFileRunner.Run(command);
		});

		_startMenuContent = new StartMenuContent(actionBar);

		var searchResults = viewModelObservable.Select(vm => vm.SearchResults).DistinctUntilChanged();

		viewModelObservable.Select(vm => vm.AllApps).DistinctUntilChanged().UnbundleMany(i => i.Key).RemoveIndex().SubscribeDebug(itemObservable =>
		{
			_startMenuContent.AddApplication(
				itemObservable.Key.Key,
				itemObservable.DistinctUntilChanged().Select(i => i.Value),
				searchResults.Select(sr => sr.IndexOf(itemObservable.Key.Value.DesktopFile.Name)).DistinctUntilChanged().TakeUntil(itemObservable.TakeLast(1)));

			itemObservable.TakeLast(1).SubscribeDebug(i => _startMenuContent.RemoveApplication(i.Value));
		});

		// TODO: Needs to be registered using BEFORE
		_root.ObserveEvent(w => w.Signal_KeyPressEvent(GConnectFlags.G_CONNECT_DEFAULT), OnKeyPressEvent);
		_root.ObserveEvent(w => w.Signal_Show()).SubscribeDebug(_ => _startMenuContent.HandleWindowShown());
		_root.ObserveEvent(w => w.Signal_ConfigureEvent()).SubscribeDebug(e => _configureEventSubject.OnNext(e.Event.Dereference()));
		_root.ObserveEvent(_startMenuContent.DesktopFileAction).SubscribeDebug(a => DesktopFileRunner.Run(a));
		_root.ObserveEvent(_startMenuContent.AppOrderingChanged).SubscribeDebug(t => store.Dispatch(new UpdateStartMenuPinnedAppOrderingAction(t)));
		_root.ObserveEvent(_startMenuContent.ToggleStartMenuPinning).SubscribeDebug(f => store.Dispatch(new ToggleStartMenuPinningAction(f)));
		_root.ObserveEvent(_startMenuContent.ToggleTaskbarPinning).SubscribeDebug(taskbarService.ToggleDesktopFilePinning);
		_root.ObserveEvent(_startMenuContent.SearchTextUpdated).SubscribeDebug(text => store.Dispatch(new UpdateStartMenuSearchTextAction(text)));
		_root.ObserveEvent(_startMenuContent.AppLaunch).SubscribeDebug(desktopFile =>
		{
			ToggleVisibility();
			DesktopFileRunner.Run(desktopFile);
		});

		store.Actions
			.OfType<WindowFocusedChangedAction>()
			.ObserveOn(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.Where(action => _root.IsVisible() && action.WindowRef.Id != _root.GetWindow().GetXid().Value)
			.SubscribeDebug(_ => ToggleVisibility());

		store.Actions.OfType<StartMenuOpenedAction>()
			.ObserveOn(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.SubscribeDebug(_ => ToggleVisibility());

		_revealer = GtkRevealerHandle.New()
			.AddMany(_startMenuContent.Widget)
			.SetTransitionDuration(250)
			.SetTransitionType(GtkRevealerTransitionType.GTK_REVEALER_TRANSITION_TYPE_SLIDE_UP)
			.Show()
			.SetValign(GtkAlign.GTK_ALIGN_END);

		_root.SetSizeRequest(640, 725);
		_root.Add(_revealer);
		viewModelObservable.Connect();
	}

	public void ToggleVisibility()
	{
		_root.GetDisplay().GetPointer(out _, out var x, out var y, out _);
		var eventMonitor = _root.GetDisplay().GetMonitorAtPoint(x, y);

		if (_root.IsVisible())
		{
			_revealer.SetRevealChild(false);
			Observable.Timer(TimeSpan.FromMilliseconds(250)).ObserveOn(GLibExt.Scheduler).SubscribeDebug(_ => _root.Hide());
		}
		else
		{
			_root.Show();
			_root.CenterOnScreenAtBottom(eventMonitor);
			_revealer.SetRevealChild(true);
		}
	}

	private void OnKeyPressEvent(GtkWidgetHandleSignalStructs.KeyPressEventSignal args)
	{
		var e = args.Event.Dereference();

		if (e.keyval == GdkConstants.KEY_Escape)
		{
			ToggleVisibility();
			args.ReturnValue = true;
		}
		else if (_startMenuContent.HandleKeyPress(e.keyval))
		{
			args.ReturnValue = true;
		}
	}
}
