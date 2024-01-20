using System.Reactive.Linq;
using Glimpse.UI.State;
using Gtk;

namespace Glimpse.UI.Components.Shared;

public class Chip : Box
{
	private readonly Label _label;

	public Chip(string text, IObservable<StartMenuAppFilteringChip> viewModelObs)
	{
		_label = new Label(text);
		_label.AddClass("chip__label");

		var labelEventBox = new EventBox();
		labelEventBox.Add(_label);
		labelEventBox.AddButtonStates();

		var labelEventBoxBem = BlockElementModifier.Create(labelEventBox, "chip__label-container");
		viewModelObs.Select(vm => vm.IsSelected).DistinctUntilChanged().Subscribe(isSelected => labelEventBoxBem.UpdateSelected(isSelected));
		viewModelObs.Select(vm => vm.IsVisible).DistinctUntilChanged().Subscribe(visible => this.Visible = visible);

		Add(labelEventBox);
		this.AddClass("chip__container");
	}
}
