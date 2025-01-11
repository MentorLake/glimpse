using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ContextMenu;
using MentorLake.Gdk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.SystemTray;

public class SystemTrayIcon
{
	public GtkWidgetHandle Widget => _root;

	private readonly GtkButtonHandle _root;

	private readonly Subject<int> _menuItemActivatedSubject = new();
	private readonly Subject<(int, int)> _applicationActivated = new();

	public SystemTrayIcon(IObservable<SystemTrayItemViewModel> viewModelObservable)
	{
		_root = GtkButtonHandle.New();
		_root.SetValign(GtkAlign.GTK_ALIGN_CENTER);
		_root.GetStyleContext().AddClass("system-tray__icon");

		var image = GtkImageHandle.New();
		image.SetValign(GtkAlign.GTK_ALIGN_CENTER);
		image.BindViewModel(viewModelObservable.Select(vm => vm.Icon).DistinctUntilChanged(), 24);
		_root.Add(image);

		viewModelObservable.TakeUntilDestroyed(_root).Select(s => s.Tooltip).DistinctUntilChanged().Subscribe(t =>
		{
			_root.SetTooltipText(t);
			_root.SetHasTooltip(!string.IsNullOrEmpty(_root.GetTooltipText()));
		});

		var contextMenu = ContextMenuFactory.Create<SystemTrayContextMenuItemViewModel>(_root);
		viewModelObservable.TakeUntilDestroyed(_root).Select(s => s.ContextMenuItems).DistinctUntilChanged().Subscribe(items => contextMenu.UpdateItems(items));

		contextMenu.ItemActivated.Subscribe(i =>
		{
			_menuItemActivatedSubject.OnNext(i.DBusId);
		});

		_root.ObserveEvent(w => w.Signal_ButtonReleaseEvent())
			.Select(a => a.Event.Dereference())
			.WithLatestFrom(viewModelObservable)
			.Where(t => t.Second.CanActivate && t.First.button == 1)
			.Select(t => t.First)
			.Subscribe(e => _applicationActivated.OnNext(((int)e.x_root, (int)e.y_root)));

		_root.ObserveEvent(w => w.Signal_ButtonReleaseEvent())
			.Select(a => a.Event.Dereference())
			.WithLatestFrom(viewModelObservable)
			.Where(t => !t.Second.CanActivate && t.First.button == 1)
			.Subscribe(_ => contextMenu.Widget.Activate());

		_root.ShowAll();

		_root.Signal_DestroyEvent().Take(1).Subscribe(_ =>
		{
			_menuItemActivatedSubject.OnCompleted();
			_applicationActivated.OnCompleted();
		});
	}

	public IObservable<int> MenuItemActivated => _menuItemActivatedSubject;
	public IObservable<(int, int)> ApplicationActivated => _applicationActivated;
}
