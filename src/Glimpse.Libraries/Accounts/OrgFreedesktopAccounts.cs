using Glimpse.Libraries.DBus;
using Glimpse.Libraries.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.Accounts;

public class OrgFreedesktopAccounts
{
	private const string Interface = "org.freedesktop.Accounts";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgFreedesktopAccounts(DBusConnections dBusConnections)
	{
		_connection = dBusConnections.System;
		_destination = "org.freedesktop.Accounts";
		_path = "/org/freedesktop/Accounts";
	}

	public Task<ObjectPath[]> ListCachedUsersAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ao);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ListCachedUsers");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath> FindUserByIdAsync(long id)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_o);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "FindUserById", "x");
			writer.WriteInt64(id);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath> FindUserByNameAsync(string name)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_o);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "FindUserByName", "s");
			writer.WriteString(name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath> CreateUserAsync(string name, string fullname, int accountType)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_o);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CreateUser", "ssi");
			writer.WriteString(name);
			writer.WriteString(fullname);
			writer.WriteInt32(accountType);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath> CacheUserAsync(string name)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_o);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "CacheUser", "s");
			writer.WriteString(name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task UncacheUserAsync(string name)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "UncacheUser", "s");
			writer.WriteString(name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task DeleteUserAsync(long id, bool removeFiles)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "DeleteUser", "xb");
			writer.WriteInt64(id);
			writer.WriteBool(removeFiles);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public ValueTask<IDisposable> WatchUserAddedAsync(Action<Exception?, ObjectPath> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "UserAdded",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_o, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchUserDeletedAsync(Action<Exception?, ObjectPath> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "UserDeleted",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_o, handler, emitOnCapturedContext);
	}

	public Task<string> GetDaemonVersionPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("DaemonVersion");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetHasNoUsersPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("HasNoUsers");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetHasMultipleUsersPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("HasMultipleUsers");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<ObjectPath[]> GetAutomaticLoginUsersPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ao);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("AutomaticLoginUsers");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<Properties> GetAllPropertiesAsync()
	{
		return _connection.CallMethodAsync(CreateGetAllMessage(), (message, state) =>
		{
			var reader = message.GetBodyReader();
			return ReadProperties(ref reader);
		});

		MessageBuffer CreateGetAllMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "GetAll", "s");
			writer.WriteString(Interface);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	private static Properties ReadProperties(ref Reader reader, List<string>? changed = null)
	{
		var props = new Properties();
		var headersEnd = reader.ReadArrayStart(DBusType.Struct);
		while (reader.HasNext(headersEnd))
		{
			switch (reader.ReadString())
			{
				case "DaemonVersion":
					reader.ReadSignature("s");
					props.DaemonVersion = reader.ReadString();
					changed?.Add("DaemonVersion");
					break;
				case "HasNoUsers":
					reader.ReadSignature("b");
					props.HasNoUsers = reader.ReadBool();
					changed?.Add("HasNoUsers");
					break;
				case "HasMultipleUsers":
					reader.ReadSignature("b");
					props.HasMultipleUsers = reader.ReadBool();
					changed?.Add("HasMultipleUsers");
					break;
				case "AutomaticLoginUsers":
					reader.ReadSignature("ao");
					props.AutomaticLoginUsers = reader.ReadArray_ao();
					changed?.Add("AutomaticLoginUsers");
					break;
			}
		}

		return props;
	}

	public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<Properties>> handler, bool emitOnCapturedContext = true)
	{
		return SignalHelper.WatchPropertiesChangedAsync(_connection, _destination, _path, Interface, ReadMessage, handler, emitOnCapturedContext);

		static PropertyChanges<Properties> ReadMessage(Message message, object? _)
		{
			var reader = message.GetBodyReader();
			reader.ReadString();
			List<string> changed = new();
			return new PropertyChanges<Properties>(ReadProperties(ref reader, changed), changed.ToArray(), reader.ReadArray_as());
		}
	}

	public class Properties
	{
		public string DaemonVersion { get; set; }
		public bool HasNoUsers { get; set; }
		public bool HasMultipleUsers { get; set; }
		public ObjectPath[] AutomaticLoginUsers { get; set; }
	}
}
