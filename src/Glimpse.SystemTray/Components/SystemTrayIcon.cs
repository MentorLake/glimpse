using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.SystemTray.Components;

public class SystemTrayIcon : Button
{
	private readonly Subject<int> _menuItemActivatedSubject = new();
	private readonly Subject<(int, int)> _applicationActivated = new();

	public SystemTrayIcon(IObservable<SystemTrayItemViewModel> viewModelObservable)
	{
		var contextMenu = ContextMenuFactory.Create(this, viewModelObservable.Select(s => s.ContextMenuItems));

		Valign = Align.Center;
		StyleContext.AddClass("system-tray__icon");

		var image = new Image();
		image.Valign = Align.Center;
		image.BindViewModel(viewModelObservable.Select(vm => vm.Icon).DistinctUntilChanged(), 24);
		Add(image);

		viewModelObservable.TakeUntilDestroyed(this).Select(s => s.Tooltip).DistinctUntilChanged().Subscribe(t =>
		{
			TooltipText = t;
			HasTooltip = !string.IsNullOrEmpty(TooltipText);
		});

		contextMenu.ItemActivated.Subscribe(i =>
		{
			_menuItemActivatedSubject.OnNext(i.DBusId);
		});

		this.ObserveEvent(w => w.Events().ButtonReleaseEvent)
			.WithLatestFrom(viewModelObservable)
			.Where(t => t.Second.CanActivate && t.First.Event.Button == 1)
			.Select(t => t.First)
			.Subscribe(e => _applicationActivated.OnNext(((int)e.Event.XRoot, (int)e.Event.YRoot)));

		this.ObserveEvent(w => w.Events().ButtonReleaseEvent)
			.WithLatestFrom(viewModelObservable)
			.Where(t => !t.Second.CanActivate && t.First.Event.Button == 1)
			.Subscribe(_ => contextMenu.Popup());

		ShowAll();
	}

	public IObservable<int> MenuItemActivated => _menuItemActivatedSubject;
	public IObservable<(int, int)> ApplicationActivated => _applicationActivated;

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		_menuItemActivatedSubject.OnCompleted();
		_applicationActivated.OnCompleted();
	}
}
