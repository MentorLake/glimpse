using System.Collections.Immutable;
using Glimpse.Common.Freedesktop.DBus;
using Glimpse.Common.Gtk;

namespace Glimpse.SystemTray.Components;

public class SystemTrayViewModel
{
	public ImmutableList<SystemTrayItemViewModel> Items = ImmutableList<SystemTrayItemViewModel>.Empty;
}

public class SystemTrayItemViewModel
{
	public string Id { get; set; }
	public string Tooltip { get; set; }
	public ImageViewModel Icon { get; set; }
	public DbusObjectDescription StatusNotifierItemDescription { get; init; }
	public DbusObjectDescription DbusMenuDescription { get; init; }
	public DbusMenuItem RootMenuItem { get; init; }
	public bool CanActivate { get; set; }
}
