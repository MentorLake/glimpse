using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using MentorLake.Gdk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared.ContextMenu;

public static class ContextMenuFactory
{
	public static ContextMenu<T> Create<T>(GtkWidgetHandle parent) where T : IContextMenuItemViewModel<T>
	{
		var contextMenu = new ContextMenu<T>();
		contextMenu.Widget.SetReserveToggleSize(false);

		 var buttonPressObs = parent.ObserveEvent(w => w.Signal_ButtonReleaseEvent())
		 	.Where(e => e.Event.Dereference() is { button: 3, type: GdkEventType.GDK_BUTTON_RELEASE })
		 	.Do(e => e.ReturnValue = true)
		 	.Select(_ => true);

		 var popupMenuObs = parent.ObserveEvent(w => w.Signal_PopupMenu())
		 	.Select(_ => true);

		 buttonPressObs.Merge(popupMenuObs).Where(_ => contextMenu.HasItems).Subscribe(_ =>
		 {
		 	contextMenu.PopupContextMenu(parent);
		 });

		return contextMenu;
	}
}
