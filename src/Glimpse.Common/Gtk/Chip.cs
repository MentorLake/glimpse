using System.Reactive.Linq;
using Gtk;

namespace Glimpse.Common.Gtk;

public class Chip
{
	public static Widget Create(string text, IObservable<StartMenuAppFilteringChip> viewModelObs)
	{
		var container = new Box(Orientation.Horizontal, 0);
		var label = new Label(text);
		label.AddClass("chip__label");

		var labelEventBox = new EventBox();
		labelEventBox.Add(label);
		labelEventBox.AddButtonStates();

		var labelEventBoxBem = BlockElementModifier.Create(labelEventBox, "chip__label-container");
		viewModelObs.Select(vm => vm.IsSelected).DistinctUntilChanged().Subscribe(isSelected => labelEventBoxBem.UpdateSelected(isSelected));
		viewModelObs.Select(vm => vm.IsVisible).DistinctUntilChanged().Subscribe(visible => container.Visible = visible);

		container.Add(labelEventBox);
		container.AddClass("chip__container");
		return container;
	}
}
