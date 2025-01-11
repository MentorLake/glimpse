using System.Reactive.Linq;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.StatusNotifierWatcher;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Services.SystemTray;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.UI.Components.SystemTray;

public class SystemTrayBox
{
	public GtkWidgetHandle Widget => _root;

	private readonly GtkBoxHandle _root;

	public SystemTrayBox(ReduxStore store, StatusNotifierWatcherService statusNotifierWatcherService)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		_root.GetStyleContext().AddClass("system-tray__taskbar-container");

		var volumeIcon = GtkImageHandle.New();
		volumeIcon.SetFromIconName("audio-volume-medium", GtkIconSize.GTK_ICON_SIZE_DIALOG);
		volumeIcon.SetPixelSize(24);

		var volumeButton = GtkButtonHandle.New()
			.AddClass("system-tray__icon")
			.AddMany(volumeIcon);

		volumeButton.ObserveEvent(x => x.Signal_ButtonReleaseEvent())
			.Where(e => e.Event.Dereference().button == 1)
			.WithLatestFrom(store.Select(SystemTraySelectors.VolumeCommand))
			.Subscribe(t => DesktopFileRunner.Run(t.Second));

		_root.PackEnd(volumeButton, false, false, 0);

		store
			.Select(SystemTrayViewModelSelector.ViewModel)
			.TakeUntilDestroyed(_root)
			.ObserveOn(GLibExt.Scheduler)
			.Select(x => x.Items)
			.DistinctUntilChanged()
			.UnbundleMany(i => i.Id)
			.RemoveIndex()
			.Subscribe(itemObservable =>
			{
				var systemTrayIcon = new SystemTrayIcon(itemObservable);
				_root.PackStart(systemTrayIcon.Widget, false, false, 0);
				_root.ShowAll();

				systemTrayIcon.MenuItemActivated.TakeUntilDestroyed(_root).WithLatestFrom(itemObservable).Subscribe(t =>
				{
					statusNotifierWatcherService.ActivateMenuItemAsync(t.Second.DbusMenuDescription, t.First);
				});

				systemTrayIcon.ApplicationActivated.TakeUntilDestroyed(_root).WithLatestFrom(itemObservable).Subscribe(t =>
				{
					statusNotifierWatcherService.ActivateSystemTrayItemAsync(t.Second.StatusNotifierItemDescription, t.First.Item1, t.First.Item2);
				});

				itemObservable.TakeLast(1).Subscribe(_ => systemTrayIcon.Widget.Destroy());
			});
	}
}
