using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GLib;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Glimpse.StartMenu.Components;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Taskbar.Components.StartMenuIcon;

public class StartMenuIcon : EventBox
{
	public StartMenuIcon(ReduxStore store, StartMenuWindow startMenuWindow)
	{
		var viewModelObservable = store.Select(StartMenuIconViewModelSelectors.s_viewModel)
			.TakeUntilDestroyed(this)
			.ObserveOn(new SynchronizationContextScheduler(new GLibSynchronizationContext(), false))
			.Replay(1)
			.AutoConnect();

		startMenuWindow.ObserveEvent(w => w.Events().Shown).TakeUntilDestroyed(this)
			.Merge(startMenuWindow.ObserveEvent(w => w.Events().Hidden).TakeUntilDestroyed(this))
			.Merge(startMenuWindow.WindowMoved.TakeUntilDestroyed(this).Select(x => (object) x))
			.Subscribe(_ =>
			{
				if (!startMenuWindow.IsVisible || Display.GetMonitorAtWindow(Window) != Display.GetMonitorAtWindow(startMenuWindow.Window))
				{
					StyleContext.RemoveClass("start-menu__launch-icon--open");
				}
				else
				{
					StyleContext.AddClass("start-menu__launch-icon--open");
				}
			});

		Expand = false;
		Valign = Align.Center;
		Halign = Align.Center;
		CanFocus = false;
		this.AddClass("start-menu__launch-icon");

		var iconObservable = viewModelObservable.Select(v => v.StartMenuLaunchIconName).DistinctUntilChanged().Select(n => new ImageViewModel() { IconNameOrPath = n });
		var image = new Image();
		image.SetSizeRequest(42, 42);
		Add(image);

		this.AppIcon(image, iconObservable, 32);
		this.ObserveEvent(w => w.Events().ButtonReleaseEvent).Where(e => e.Event.Button == 1).Subscribe(e =>
		{
			startMenuWindow.ToggleVisibility();
			e.RetVal = true;
		});

		var contextMenu = ContextMenuFactory.Create(this, viewModelObservable.Select(i => i.ContextMenuItems));
		contextMenu.ItemActivated.Subscribe(i => DesktopFileRunner.Run(i.Executable + " " + i.Arguments));
	}
}
