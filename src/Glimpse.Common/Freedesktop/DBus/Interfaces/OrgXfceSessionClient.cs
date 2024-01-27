using Glimpse.Common.Freedesktop.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop.DBus.Interfaces;

public class OrgXfceSessionClient
{
	private const string Interface = "org.xfce.Session.Client";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgXfceSessionClient(Connection connection, string applicationId)
	{
		_connection = connection;
		_destination = "org.xfce.SessionManager";
		_path = "/org/xfce/SessionClients/" + applicationId;
	}

	public Task<string> GetIDAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetID");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<uint> GetStateAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetState");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<Dictionary<string, DBusVariantItem>> GetAllSmPropertiesAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_aesv);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetAllSmProperties");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<Dictionary<string, DBusVariantItem>> GetSmPropertiesAsync(string[] names)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_aesv);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetSmProperties", "as");
			writer.WriteArray_as(names);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SetSmPropertiesAsync(Dictionary<string, DBusVariantItem> properties)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SetSmProperties", "a{sv}");
			writer.WriteDictionary_aesv(properties);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task DeleteSmPropertiesAsync(string[] names)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "DeleteSmProperties", "as");
			writer.WriteArray_as(names);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task TerminateAsync()
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Terminate");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task EndSessionResponseAsync(bool is_ok, string reason)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "EndSessionResponse", "bs");
			writer.WriteBool(is_ok);
			writer.WriteString(reason);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public ValueTask<IDisposable> WatchStateChangedAsync(Action<Exception?, (uint old_state, uint new_state)> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "StateChanged",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_uu, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchSmPropertyChangedAsync(Action<Exception?, (string name, DBusVariantItem value)> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "SmPropertyChanged",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_sv, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchSmPropertyDeletedAsync(Action<Exception?, string> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "SmPropertyDeleted",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_s, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchQueryEndSessionAsync(Action<Exception?, uint> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "QueryEndSession",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_u, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchEndSessionAsync(Action<Exception?, uint> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "EndSession",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_u, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchCancelEndSessionAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "CancelEndSession",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchStopAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "Stop",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}
}
