using Glimpse.Common.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.XSessionManagement;

public class OrgXfceSessionManager
{
	private const string Interface = "org.xfce.Session.Manager";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgXfceSessionManager(Connection connection)
	{
		_connection = connection;
		_destination = "org.xfce.SessionManager";
		_path = "/org/xfce/SessionManager";
	}

	public Task<(string name, string version, string vendor)> GetInfoAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_sss);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetInfo");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath[]> ListClientsAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ao);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ListClients");
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

	public Task CheckpointAsync(string session_name)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Checkpoint", "s");
			writer.WriteString(session_name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task LogoutAsync(bool show_dialog, bool allow_save)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Logout", "bb");
			writer.WriteBool(show_dialog);
			writer.WriteBool(allow_save);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task ShutdownAsync(bool allow_save)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Shutdown", "b");
			writer.WriteBool(allow_save);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> CanShutdownAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CanShutdown");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task RestartAsync(bool allow_save)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Restart", "b");
			writer.WriteBool(allow_save);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> CanRestartAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CanRestart");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SuspendAsync()
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Suspend");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> CanSuspendAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CanSuspend");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task HibernateAsync()
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Hibernate");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> CanHibernateAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CanHibernate");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task HybridSleepAsync()
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "HybridSleep");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> CanHybridSleepAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CanHybridSleep");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SwitchUserAsync()
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SwitchUser");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath> RegisterClientAsync(string app_id, string client_startup_id)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_o);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "RegisterClient", "ss");
			writer.WriteString(app_id);
			writer.WriteString(client_startup_id);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task UnregisterClientAsync(ObjectPath client_id)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "UnregisterClient", "o");
			writer.WriteObjectPath(client_id);
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

	public ValueTask<IDisposable> WatchClientRegisteredAsync(Action<Exception?, ObjectPath> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "ClientRegistered",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_o, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchShutdownCancelledAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "ShutdownCancelled",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}
}
