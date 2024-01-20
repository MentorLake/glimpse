using System.Collections.Immutable;
using Glimpse.Freedesktop.DBus;
using Glimpse.Freedesktop.DBus.Introspection;
using Glimpse.UI.State;

namespace Glimpse.UI.Components.SystemTray;

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
