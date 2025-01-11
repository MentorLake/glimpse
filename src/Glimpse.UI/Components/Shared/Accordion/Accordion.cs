using Glimpse.Libraries.Gtk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared.Accordion;

public class Accordion
{
	private readonly List<AccordionSection> _sections = new();
	private readonly GtkBoxHandle _sectionsContainer;
	private readonly GtkBoxHandle _root;

	public GtkBoxHandle Widget => _root;

	public Accordion()
	{
		_sectionsContainer = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0);

		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
			.AddMany(GtkScrolledWindowHandle.New(null, null)
				.Prop(w => w.SetPolicy(GtkPolicyType.GTK_POLICY_NEVER, GtkPolicyType.GTK_POLICY_AUTOMATIC))
				.Prop(w => w.SetHexpand(true))
				.Prop(w => w.SetVexpand(true))
				.AddMany(_sectionsContainer));
	}

	public void AddSection(string sectionName, GtkWidgetHandle sectionHeader)
	{
		var sectionItemsContainer = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 8)
			.Prop(w => w.SetVisible(false));

		var sectionContainer = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 8)
			.Prop(w => w.SetHalign(GtkAlign.GTK_ALIGN_FILL))
			.AddMany(GtkEventBoxHandle.New()
				.AddButtonStates()
				.AddClass("button")
				.AddMany(sectionHeader)
				.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ =>
				{
					foreach (var s in _sections.Where(s => s.Name != sectionName)) s.ItemContainer.SetVisible(false);
					sectionItemsContainer.SetVisible(!sectionItemsContainer.IsVisible());
				}))
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

	public void AddItemToSection(string sectionName, GtkWidgetHandle item)
	{
		if (_sections.FirstOrDefault(s => s.Name == sectionName) is { } section)
		{
			section.ItemContainer.Add(item);
		}
	}

	public void RemoveItemFromSection(string sectionName, GtkWidgetHandle item)
	{
		item.Destroy();
	}

	public void ShowFirstSection()
	{
		foreach (var section in _sections)
		{
			section.ItemContainer.SetVisible(false);
		}

		if (_sections.FirstOrDefault()?.ItemContainer is { } firstItemContainer)
		{
			firstItemContainer.SetVisible(true);
		}
	}
}
