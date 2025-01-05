using System.Reactive.Linq;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.StatusNotifierWatcher;
using Glimpse.Common.System.Reactive;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.SystemTray.Components;

public class SystemTrayBox
{
	public Widget Widget => _root;

	private readonly Box _root;

	public SystemTrayBox(ReduxStore store, StatusNotifierWatcherService statusNotifierWatcherService)
	{
		_root = new Box(Orientation.Horizontal, 0);
		_root.StyleContext.AddClass("system-tray__taskbar-container");

		var volumeIcon = new Image();
		volumeIcon.SetFromIconName("audio-volume-medium", IconSize.Dialog);
		volumeIcon.PixelSize = 24;

		var volumeButton = new Button()
			.AddClass("system-tray__icon")
			.AddMany(volumeIcon);

		volumeButton.ObserveEvent(w => w.Events().ButtonReleaseEvent)
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
