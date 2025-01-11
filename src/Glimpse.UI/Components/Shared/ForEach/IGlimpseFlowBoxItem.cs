using MentorLake.GdkPixbuf;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared.ForEach;

public interface IGlimpseFlowBoxItem
{
	GtkWidgetHandle Widget { get; }
	GdkPixbufHandle DragIcon { get; }
}
