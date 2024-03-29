using System.Collections.Immutable;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;

namespace Glimpse.Taskbar.Components.StartMenuIcon;

internal record StartMenuIconContextMenuItem : IContextMenuItemViewModel<StartMenuIconContextMenuItem>
{
	public string Executable { get; set; }
	public string Arguments { get; set; } = "";
	public string DisplayText { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public string Id { get; set; }
	public ImmutableList<StartMenuIconContextMenuItem> Children { get; set; } = ImmutableList<StartMenuIconContextMenuItem>.Empty;
}

internal record StartMenuIconViewModel
{
	public ImmutableList<StartMenuIconContextMenuItem> ContextMenuItems { get; set; } = ImmutableList<StartMenuIconContextMenuItem>.Empty;
	public string StartMenuLaunchIconName { get; set; }
}
