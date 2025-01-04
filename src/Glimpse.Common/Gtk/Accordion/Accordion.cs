using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Gtk.Accordion;

public class Accordion : Bin
{
	private readonly List<AccordionSection> _sections = new();
	private readonly Box _sectionsContainer;

	public Accordion()
	{
		_sectionsContainer = new Box(Orientation.Vertical, 0);

		var accordion = new ScrolledWindow()
			.Prop(w => w.HscrollbarPolicy = PolicyType.Never)
			.Prop(w => w.Expand = true)
			.AddMany(_sectionsContainer);

		Add(accordion);
	}

	public void AddSection(string sectionName, Widget sectionHeader)
	{
		var sectionItemsContainer = new Box(Orientation.Vertical, 8);
		sectionItemsContainer.Visible = false;

		var sectionContainer = new Box(Orientation.Vertical, 8)
			.Prop(w => w.Halign = Align.Fill)
			.AddMany(new EventBox()
				.AddButtonStates()
				.AddClass("button")
				.AddMany(sectionHeader)
				.Prop(w => w.ObserveEvent<Widget, ButtonReleaseEventArgs>(w1 => w1.Events().ButtonReleaseEvent).Subscribe(_ =>
				{
					foreach (var s in _sections.Where(s => s.Name != sectionName)) s.ItemContainer.Visible = false;
					sectionItemsContainer.Visible = !sectionItemsContainer.Visible;
				})))
			.AddMany(sectionItemsContainer);

		_sections.Add(new AccordionSection() { Name = sectionName, Root = sectionContainer, ItemContainer = sectionItemsContainer });
		_sectionsContainer.Add(sectionContainer);
		sectionContainer.ShowAll();
	}

	public void RemoveSection(string sectionName)
	{
		if (_sections.FirstOrDefault(s => s.Name == sectionName) is { } section)
		{
			section.Root.Destroy();
			_sections.Remove(section);
		}
	}

	public void AddItemToSection(string sectionName, Widget item)
	{
		if (_sections.FirstOrDefault(s => s.Name == sectionName) is { } section)
		{
			section.ItemContainer.Add(item);
		}
	}

	public void RemoveItemFromSection(string sectionName, Widget item)
	{
		item.Destroy();
	}

	public void ShowFirstSection()
	{
		foreach (var section in _sections)
		{
			section.ItemContainer.Visible = false;
		}

		if (_sections.FirstOrDefault()?.ItemContainer is { } firstItemContainer)
		{
			firstItemContainer.Visible = true;
		}
	}
}
