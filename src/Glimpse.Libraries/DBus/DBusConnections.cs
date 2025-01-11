using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.DBus;

public class DBusConnections
{
	public Connection Session { get; set; }
	public Connection System { get; set; }
}
