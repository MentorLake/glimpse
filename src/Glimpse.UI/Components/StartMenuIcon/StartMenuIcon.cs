using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ContextMenu;
using MentorLake.Gdk;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.UI.Components.StartMenuIcon;

public class StartMenuIcon
{
	private readonly Subject<GdkRectangle> _startMenuButtonClicked = new();
	private readonly GtkEventBoxHandle _root;

	public GtkWidgetHandle Widget => _root;
	public IObservable<GdkRectangle> StartMenuButtonClicked => _startMenuButtonClicked;

	public void StartMenuClosed()
	{
		Widget.GetStyleContext().RemoveClass("start-menu__launch-icon--open");
	}

	public void StartMenuOpened()
	{
		Widget.GetStyleContext().AddClass("start-menu__launch-icon--open");
	}

	public StartMenuIcon(ReduxStore store)
	{
		var image = GtkImageHandle.New()
			.SetValign(GtkAlign.GTK_ALIGN_CENTER)
			.SetHalign(GtkAlign.GTK_ALIGN_CENTER);

		_root = GtkEventBoxHandle.New()
			.SetCanFocus(false)
			.AddClass("start-menu__launch-icon")
			.SetSizeRequest(42, 42)
			.Add(image);

		var viewModelObservable = store.Select(StartMenuIconViewModelSelectors.s_viewModel)
			.TakeUntilDestroyed(_root)
			.ObserveOn(GLibExt.Scheduler)
			.Replay(1)
			.AutoConnect();

		var iconObservable = viewModelObservable
			.Select(v => v.StartMenuLaunchIconName)
			.DistinctUntilChanged()
			.Select(n => new ImageViewModel() { IconNameOrPath = n });

		_root.AppIcon(image, iconObservable, 36);

		_root.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).Where(e => e.Event.Dereference().button == 1).Subscribe(e =>
		{
			_root.GetAllocation(out var allocation);
			_startMenuButtonClicked.OnNext(allocation.Value);
			e.ReturnValue = true;
		});

		var contextMenu = ContextMenuFactory.Create<StartMenuIconContextMenuItem>(_root);
		contextMenu.ItemActivated.Subscribe(i => DesktopFileRunner.Run(i.Executable + " " + i.Arguments));
		viewModelObservable.Select(i => i.ContextMenuItems).DistinctUntilChanged().Subscribe(items => contextMenu.UpdateItems(items));
	}
}
