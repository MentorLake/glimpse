using System.Collections.Immutable;
using Glimpse.Libraries.DBus;
using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ContextMenu;

namespace Glimpse.UI.Components.SystemTray;

public record SystemTrayViewModel
{
	public ImmutableList<SystemTrayItemViewModel> Items = ImmutableList<SystemTrayItemViewModel>.Empty;
}

public record SystemTrayContextMenuItemViewModel : IContextMenuItemViewModel<SystemTrayContextMenuItemViewModel>
{
	public int DBusId { get; set; }
	public string DisplayText { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public string Id { get; set; }
	public ImmutableList<SystemTrayContextMenuItemViewModel> Children { get; set; } = ImmutableList<SystemTrayContextMenuItemViewModel>.Empty;
}

public record SystemTrayItemViewModel
{
	public string Id { get; set; }
	public string Tooltip { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public DbusObjectDescription StatusNotifierItemDescription { get; init; }
	public DbusObjectDescription DbusMenuDescription { get; init; }
	public bool CanActivate { get; set; }
	public ImmutableList<SystemTrayContextMenuItemViewModel> ContextMenuItems { get; set; } = ImmutableList<SystemTrayContextMenuItemViewModel>.Empty;
}
