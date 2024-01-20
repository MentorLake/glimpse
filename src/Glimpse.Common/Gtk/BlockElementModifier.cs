using Gtk;

namespace Glimpse.UI.Components.Shared;

public class BlockElementModifier
{
	private readonly Widget _widget;
	private readonly string _blockElement;
	private readonly LinkedList<string> _modifiers = new();

	public BlockElementModifier(Widget widget, string blockElement)
	{
		_widget = widget;
		_blockElement = blockElement;
	}

	public static BlockElementModifier Create(Widget widget, string blockElement)
	{
		widget.AddClass(blockElement);
		return new BlockElementModifier(widget, blockElement);
	}

	public BlockElementModifier AddModifier(string modifier)
	{
		if (_modifiers.Contains(modifier)) return this;
		_modifiers.AddLast(modifier);
		_widget.AddClass(_blockElement + "--" + modifier);
		return this;
	}

	public BlockElementModifier RemoveModifier(string modifier)
	{
		if (!_modifiers.Contains(modifier)) return this;
		_modifiers.Remove(modifier);
		_widget.RemoveClass(_blockElement + "--" + modifier);
		return this;
	}

	public BlockElementModifier Select()
	{
		return RemoveModifier("unselected").AddModifier("selected");
	}

	public BlockElementModifier Unselect()
	{
		return RemoveModifier("selected").AddModifier("unselected");
	}

	public BlockElementModifier UpdateSelected(bool isSelected)
	{
		if (isSelected)
			Select();
		else
			Unselect();

		return this;
	}
}
