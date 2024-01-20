using Gtk;

namespace Glimpse.UI.Components.Shared.ForEach;

public static class ForEachExtensions
{
	public static ForEachFlowBox<TViewModel, TWidget, TKey> Create<TViewModel, TWidget, TKey>(
		IObservable<IList<TViewModel>> itemsObservable,
		Func<TViewModel, TKey> trackBy,
		Func<IObservable<TViewModel>, TWidget> widgetFactory)
			where TWidget : Widget, IForEachDraggable
			where TKey : IEquatable<TKey>
	{
		return new ForEachFlowBox<TViewModel, TWidget, TKey>(itemsObservable, trackBy, widgetFactory);
	}

	public static TViewModel GetViewModel<TViewModel, TWidget, TKey>(
		this ForEachFlowBox<TViewModel, TWidget, TKey> forEachFlowBoxWidget,
		FlowBoxChild child)
		where TWidget : Widget, IForEachDraggable
		where TViewModel : class
		where TKey : IEquatable<TKey>
	{
		return child.Data[ForEachDataKeys.Model] as TViewModel;
	}
}
