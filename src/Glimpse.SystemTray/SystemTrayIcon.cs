using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gtk;
using Menu = Gtk.Menu;

namespace Glimpse.UI.Components.SystemTray;

public class SystemTrayIcon : Button
{
	private readonly Menu _contextMenu;
	private readonly Subject<int> _menuItemActivatedSubject = new();
	private readonly Subject<(int, int)> _applicationActivated = new();

	public SystemTrayIcon(IObservable<SystemTrayItemViewModel> viewModelObservable)
	{
		_contextMenu = new Menu();

		this.CreateContextMenuObservable()
			.Where(_ => _contextMenu.Children.Any())
			.Subscribe(_ => _contextMenu.Popup());

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

		viewModelObservable.TakeUntilDestroyed(this).Select(s => s.RootMenuItem).DistinctUntilChanged().Subscribe(menuState =>
		{
			_contextMenu.RemoveAllChildren();
			DbusContextMenuHelpers.PopulateMenu(_contextMenu, menuState);
			var allMenuItems = DbusContextMenuHelpers.GetAllMenuItems(_contextMenu);
			foreach (var i in allMenuItems) i.Activated += (_, _) => _menuItemActivatedSubject.OnNext(i.GetDbusMenuItem().Id);
		});

		this.ObserveButtonRelease()
			.WithLatestFrom(viewModelObservable)
			.Where(t => t.Second.CanActivate && t.First.Event.Button == 1)
			.Select(t => t.First)
			.Subscribe(e => _applicationActivated.OnNext(((int)e.Event.XRoot, (int)e.Event.YRoot)));

		this.ObserveButtonRelease()
			.WithLatestFrom(viewModelObservable)
			.Where(t => !t.Second.CanActivate && t.First.Event.Button == 1)
			.Subscribe(_ => _contextMenu.Popup());

		ShowAll();
	}

	public IObservable<int> MenuItemActivated => _menuItemActivatedSubject;
	public IObservable<(int, int)> ApplicationActivated => _applicationActivated;

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		_menuItemActivatedSubject.OnCompleted();
		_applicationActivated.OnCompleted();
		_contextMenu.Destroy();
	}
}
