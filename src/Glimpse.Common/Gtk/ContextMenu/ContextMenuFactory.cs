using System.Collections.Immutable;
using System.Reactive.Linq;
using Gdk;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Gtk.ContextMenu;

public static class ContextMenuFactory
{
	public static ContextMenu<T> Create<T>(Widget parent, IObservable<ImmutableList<T>> viewModelObservable) where T : IContextMenuItemViewModel<T>
	{
		var contextMenu = new ContextMenu<T>();
		contextMenu.ReserveToggleSize = false;
		contextMenu.BindViewModel(viewModelObservable);

		var buttonPressObs = parent.ObserveEvent(w => w.Events().ButtonReleaseEvent)
			.Where(e => e.Event.Button == 3 && e.Event.Type == EventType.ButtonRelease)
			.Do(e => e.RetVal = true)
			.Select(_ => true);

		var popupMenuObs = parent.ObserveEvent(w => w.Events().PopupMenu)
			.Select(_ => true);

		buttonPressObs.Merge(popupMenuObs).Where(_ => contextMenu.Children.Any()).Subscribe(_ =>
		{
			contextMenu.Popup();

			contextMenu.Events().Unmapped
				.Take(1)
				.TakeUntilDestroyed(parent)
				.Subscribe(_ => parent.SetStateFlags(StateFlags.Normal, true));
		});

		return contextMenu;
	}
}
