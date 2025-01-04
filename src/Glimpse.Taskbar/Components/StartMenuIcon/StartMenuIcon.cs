using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Taskbar.Components.StartMenuIcon;

public class StartMenuIcon
{
	private readonly Subject<Rectangle> _startMenuButtonClicked = new();

	public EventBox Widget { get; }
	public IObservable<Rectangle> StartMenuButtonClicked => _startMenuButtonClicked;

	public void StartMenuClosed()
	{
		Widget.StyleContext.RemoveClass("start-menu__launch-icon--open");
	}

	public void StartMenuOpened()
	{
		Widget.StyleContext.AddClass("start-menu__launch-icon--open");
	}

	public StartMenuIcon(ReduxStore store)
	{
		Widget = new EventBox();

		var viewModelObservable = store.Select(StartMenuIconViewModelSelectors.s_viewModel)
			.TakeUntilDestroyed(Widget)
			.ObserveOn(GLibExt.Scheduler)
			.Replay(1)
			.AutoConnect();

		var iconObservable = viewModelObservable.Select(v => v.StartMenuLaunchIconName).DistinctUntilChanged().Select(n => new ImageViewModel() { IconNameOrPath = n });
		var image = new Image();

		Widget.Expand = false;
		Widget.Valign = Align.Center;
		Widget.Halign = Align.Center;
		Widget.CanFocus = false;
		Widget.AddClass("start-menu__launch-icon");
		Widget.Add(image);

		// Base the size on the height of the panel.  Listen to SizeAllocated event to update it.
		var iconWidth = (int) Widget.StyleContext.GetProperty("min-width", StateFlags.Normal).Val;
		Widget.AppIcon(image, iconObservable, iconWidth);
		Widget.ObserveEvent(w => w.Events().ButtonReleaseEvent).Where(e => e.Event.Button == 1).Subscribe(e =>
		{
			_startMenuButtonClicked.OnNext(Widget.Allocation);
			e.RetVal = true;
		});

		var contextMenu = ContextMenuFactory.Create(Widget, viewModelObservable.Select(i => i.ContextMenuItems));
		contextMenu.ItemActivated.Subscribe(i => DesktopFileRunner.Run(i.Executable + " " + i.Arguments));
	}
}
