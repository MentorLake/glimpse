using Glimpse.Libraries.DBus.Core;

namespace Glimpse.Libraries.DBus;

public class OrgFreedesktopDBusIntrospectable
{
	private const string Interface = "org.freedesktop.DBus.Introspectable";
	private readonly DBusConnection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgFreedesktopDBusIntrospectable(DBusConnection connection, string destination, string path)
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
