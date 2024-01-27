namespace Glimpse.Common.Gtk.ForEach;

public interface IForEachDraggable
{
	IObservable<ImageViewModel> IconWhileDragging { get; }
}
