using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.SystemTray.Components;

public class SystemTrayIcon
{
	public Widget Widget => _root;

	private readonly Button _root;

	private readonly Subject<int> _menuItemActivatedSubject = new();
	private readonly Subject<(int, int)> _applicationActivated = new();

	public SystemTrayIcon(IObservable<SystemTrayItemViewModel> viewModelObservable)
	{
		_root = new Button();
		_root.Valign = Align.Center;
		_root.StyleContext.AddClass("system-tray__icon");

		var image = new Image();
		image.Valign = Align.Center;
		image.BindViewModel(viewModelObservable.Select(vm => vm.Icon).DistinctUntilChanged(), 24);
		_root.Add(image);

		viewModelObservable.TakeUntilDestroyed(_root).Select(s => s.Tooltip).DistinctUntilChanged().Subscribe(t =>
		{
			_root.TooltipText = t;
			_root.HasTooltip = !string.IsNullOrEmpty(_root.TooltipText);
		});

		var contextMenu = ContextMenuFactory.Create(_root, viewModelObservable.Select(s => s.ContextMenuItems));

		contextMenu.ItemActivated.Subscribe(i =>
		{
			_menuItemActivatedSubject.OnNext(i.DBusId);
		});

		_root.ObserveEvent(w => w.Events().ButtonReleaseEvent)
			.WithLatestFrom(viewModelObservable)
			.Where(t => t.Second.CanActivate && t.First.Event.Button == 1)
			.Select(t => t.First)
			.Subscribe(e => _applicationActivated.OnNext(((int)e.Event.XRoot, (int)e.Event.YRoot)));

		_root.ObserveEvent(w => w.Events().ButtonReleaseEvent)
			.WithLatestFrom(viewModelObservable)
			.Where(t => !t.Second.CanActivate && t.First.Event.Button == 1)
			.Subscribe(_ => contextMenu.Popup());

		_root.ShowAll();

		_root.Events().Destroyed.Take(1).Subscribe(_ =>
		{
			_menuItemActivatedSubject.OnCompleted();
			_applicationActivated.OnCompleted();
		});
	}

	public IObservable<int> MenuItemActivated => _menuItemActivatedSubject;
	public IObservable<(int, int)> ApplicationActivated => _applicationActivated;
}
