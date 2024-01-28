using System.Reactive.Linq;
using GLib;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.StatusNotifierWatcher;
using Glimpse.Common.System.Reactive;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.SystemTray.Components;

public class SystemTrayBox : Box
{
	public SystemTrayBox(ReduxStore store, StatusNotifierWatcherService statusNotifierWatcherService) : base(Orientation.Horizontal, 0)
	{
		StyleContext.AddClass("system-tray__taskbar-container");

		var volumeIcon = new Image();
		volumeIcon.SetFromIconName("audio-volume-medium", IconSize.Dialog);
		volumeIcon.PixelSize = 24;

		var volumeButton = new Button()
			.AddClass("system-tray__icon")
			.AddMany(volumeIcon);

		volumeButton.ObserveEvent(w => w.Events().ButtonReleaseEvent)
			.WithLatestFrom(store.Select(SystemTraySelectors.VolumeCommand))
			.Subscribe(t => DesktopFileRunner.Run(t.Second));

		PackEnd(volumeButton, false, false, 0);

		store
			.Select(SystemTrayViewModelSelector.ViewModel)
			.TakeUntilDestroyed(this)
			.ObserveOn(new GLibSynchronizationContext())
			.Select(x => x.Items)
			.DistinctUntilChanged()
			.UnbundleMany(i => i.Id)
			.RemoveIndex()
			.Subscribe(itemObservable =>
			{
				var systemTrayIcon = new SystemTrayIcon(itemObservable);
				PackStart(systemTrayIcon, false, false, 0);
				ShowAll();

				systemTrayIcon.MenuItemActivated.TakeUntilDestroyed(this).WithLatestFrom(itemObservable).Subscribe(t =>
				{
					statusNotifierWatcherService.ActivateMenuItemAsync(t.Second.DbusMenuDescription, t.First);
				});

				systemTrayIcon.ApplicationActivated.TakeUntilDestroyed(this).WithLatestFrom(itemObservable).Subscribe(t =>
				{
					statusNotifierWatcherService.ActivateSystemTrayItemAsync(t.Second.StatusNotifierItemDescription, t.First.Item1, t.First.Item2);
				});

				itemObservable.TakeLast(1).Subscribe(_ => systemTrayIcon.Destroy());
			});
	}
}
