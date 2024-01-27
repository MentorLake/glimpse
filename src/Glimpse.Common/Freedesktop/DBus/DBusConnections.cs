using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop.DBus;

public class DBusConnections
{
	public Connection Session { get; set; }
	public Connection System { get; set; }
}
