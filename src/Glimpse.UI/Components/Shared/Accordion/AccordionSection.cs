using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared.Accordion;

public class AccordionSection
{
	public GtkBoxHandle Root { get; set; }
	public GtkBoxHandle ItemContainer { get; set; }
	public string Name { get; set; }
}
