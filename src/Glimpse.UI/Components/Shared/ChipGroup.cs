using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared;

public class ChipGroup
{
	public GtkWidgetHandle Widget { get; init; }
	public IObservable<List<string>> ChipSelectionUpdated => _chipSelectionUpdated;

	private readonly Dictionary<string, Chip> _chips = new();
	private readonly Subject<List<string>> _chipSelectionUpdated = new();
	private readonly GtkBoxHandle _root;

	public ChipGroup(int spacing)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, spacing);

		_root.Signal_Destroy().Take(1).Subscribe(_ =>
		{
			_chipSelectionUpdated.OnCompleted();
			_chipSelectionUpdated.Dispose();
		});

		Widget = _root;
	}

	public ChipGroup AddChip(string id, string text)
	{
		var newChip = new Chip(id, text);
		_chips.Add(id, newChip);
		_root.Add(newChip.Widget);

		newChip.Widget
			.Signal_ButtonReleaseEvent()
			.TakeUntilDestroyed(newChip.Widget)
			.Subscribe(_ => SelectChip(id));

		return this;
	}

	private void SelectChip(string id)
	{
		if (_chips[id].Selected) return;

		foreach (var c in _chips.Values) c.Select(false);
		_chips[id].Select(true);
		_chips[id].Widget.Show();
		_chipSelectionUpdated.OnNext(_chips.Values.Where(c => c.Selected).Select(c => c.Id).ToList());
	}

	public void Select(string id)
	{
		SelectChip(id);
	}

	public void HideChip(string id)
	{
		_chips[id].Widget.Hide();
	}
}
