using Glimpse.Libraries.Gtk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared;

public class Chip
{
	public string Id { get; }
	public bool Selected { get; private set; }

	public GtkWidgetHandle Widget { get; }

	private readonly BlockElementModifier _labelEventBoxBem;

	public Chip(string id, string text)
	{
		var root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		var label = GtkLabelHandle.New(text);
		label.AddClass("chip__label");

		var labelEventBox = GtkEventBoxHandle.New();
		labelEventBox.Add(label);
		labelEventBox.AddButtonStates();

		_labelEventBoxBem = BlockElementModifier.Create(labelEventBox, "chip__label-container");

		root.Add(labelEventBox);
		root.AddClass("chip__container");

		Id = id;
		Widget = root;
	}

	public void Select(bool enabled)
	{
		Selected = enabled;
		_labelEventBoxBem.UpdateSelected(enabled);
	}
}
