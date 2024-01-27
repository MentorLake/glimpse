using System.Collections.Immutable;

namespace Glimpse.Taskbar.Components.StartMenuIcon;

internal record StartMenuIconViewModel
{
	public ImmutableList<ContextMenuItem> ContextMenuItems { get; set; }
	public string StartMenuLaunchIconName { get; set; }
}
