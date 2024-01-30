using System.Collections.Immutable;
using Glimpse.Common.DBus;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;

namespace Glimpse.SystemTray.Components;

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
