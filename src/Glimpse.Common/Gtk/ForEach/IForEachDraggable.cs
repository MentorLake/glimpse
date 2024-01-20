using Glimpse.UI.State;

namespace Glimpse.UI.Components.Shared.ForEach;

public interface IForEachDraggable
{
	IObservable<ImageViewModel> IconWhileDragging { get; }
}
