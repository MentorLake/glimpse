using Gtk;

namespace Glimpse.Common.Gtk.ForEach;

public interface IGlimpseFlowBoxItem
{
	IObservable<ImageViewModel> IconWhileDragging { get; }
	Widget Widget { get; }
}
