using Tmds.DBus.Protocol;

namespace Glimpse.Common.DBus;

public class DBusConnections
{
	public Connection Session { get; set; }
	public Connection System { get; set; }
}
