using Glimpse.Common.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.DBus;

public class OrgFreedesktopDBusIntrospectable
{
	private const string Interface = "org.freedesktop.DBus.Introspectable";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgFreedesktopDBusIntrospectable(Connection connection, string destination, string path)
	{
		_connection = connection;
		_destination = destination;
		_path = path;
	}

	public Task<string> IntrospectAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Introspect");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}
}
