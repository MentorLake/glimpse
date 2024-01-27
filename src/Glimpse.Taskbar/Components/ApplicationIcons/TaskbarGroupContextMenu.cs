using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Common.Freedesktop.DesktopEntries;
using Glimpse.Common.Freedesktop.Xorg;
using Glimpse.Common.Gtk;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

internal class TaskbarGroupContextMenu : Menu
{
	private readonly Subject<bool> _pinSubject = new();
	private readonly Subject<AllowedWindowActions> _windowAction = new();
	private readonly Subject<DesktopFileAction> _desktopFileAction = new();
	private readonly Subject<DesktopFile> _launch = new();

	public TaskbarGroupContextMenu(IObservable<TaskbarGroupContextMenuViewModel> viewModel)
	{
		ReserveToggleSize = false;

		viewModel.Subscribe(vm =>
		{
			Children.ToList().ForEach(c => c.Destroy());
			CreateContextMenu(vm);
		});

		this.Events().Destroyed.Take(1).Subscribe(_ =>
		{
			_pinSubject.OnCompleted();
			_windowAction.OnCompleted();
			_desktopFileAction.OnCompleted();
			_launch.OnCompleted();
		});
	}

	public IObservable<bool> Pin => _pinSubject;
	public IObservable<AllowedWindowActions> WindowAction => _windowAction;
	public IObservable<DesktopFileAction> DesktopFileAction => _desktopFileAction;
	public IObservable<DesktopFile> Launch => _launch;

	private void CreateContextMenu(TaskbarGroupContextMenuViewModel viewModel)
	{
		CreateDesktopFileActions(viewModel.DesktopFile).ForEach(Add);

		if (!string.IsNullOrEmpty(viewModel.DesktopFile.FilePath))
		{
			var launchMenuItem = CreateLaunchMenuItem(viewModel, viewModel.LaunchIcon);
			Add(launchMenuItem);
			Add(CreatePinMenuItem(viewModel));
		}

		if (viewModel.CanClose && CreateCloseAction(viewModel) is { } closeAction)
		{
			Add(closeAction);
		}

		ShowAll();
	}

	private MenuItem CreatePinMenuItem(TaskbarGroupContextMenuViewModel viewModel)
	{
		var pinLabel = viewModel.IsPinned ? "Unpin from taskbar" : "Pin to taskbar";
		var pinMenuItem = ContextMenuHelper.CreateMenuItem(pinLabel, new ImageViewModel() { IconNameOrPath = viewModel.IsPinned ? "list-remove-symbolic" : "list-add-symbolic" });
		pinMenuItem.ObserveButtonRelease().Subscribe(_ => _pinSubject.OnNext(true));
		return pinMenuItem;
	}

	private MenuItem CreateLaunchMenuItem(TaskbarGroupContextMenuViewModel viewModel, ImageViewModel icon)
	{
		var pinMenuItem = ContextMenuHelper.CreateMenuItem(viewModel.DesktopFile.Name, icon);
		pinMenuItem.ObserveButtonRelease().Subscribe(_ => _launch.OnNext(viewModel.DesktopFile));
		return pinMenuItem;
	}

	private MenuItem CreateCloseAction(TaskbarGroupContextMenuViewModel viewModel)
	{
		if (viewModel.CanClose)
		{
			var menuItem = ContextMenuHelper.CreateMenuItem("Close Window", new ImageViewModel() { IconNameOrPath = "window-close-symbolic" });
			menuItem.ObserveButtonRelease().Subscribe(_ => _windowAction.OnNext(AllowedWindowActions.Close));
			return menuItem;
		}

		return null;
	}

	private List<MenuItem> CreateDesktopFileActions(DesktopFile desktopFile)
	{
		var menuItems = ContextMenuHelper.CreateDesktopFileActions(desktopFile);

		if (menuItems.Any())
		{
			menuItems.Add(new SeparatorMenuItem());

			menuItems.ForEach(m =>
			{
				var action = (DesktopFileAction)m.Data["DesktopFileAction"];
				m.ObserveButtonRelease().Subscribe(_ => _desktopFileAction.OnNext(action));
			});
		}

		return menuItems;
	}
}
