using Glimpse.Freedesktop.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Freedesktop.DBus.Interfaces;

public class OrgFreedesktopDBus
{
	private const string Interface = "org.freedesktop.DBus";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgFreedesktopDBus(Connection connection, string destination, string path)
	{
		_connection = connection;
		_destination = destination;
		_path = path;

		NameChanged = connection.WatchSignal(
			new MatchRule
			{
				Type = MessageType.Signal,
				Sender = _destination,
				Path = _path,
				Member = "NameOwnerChanged",
				Interface = Interface
			},
			ReaderExtensions.ReadMessage_sss);
	}

	public IObservable<(string, string, string)> NameChanged { get; }

	public Task<string> HelloAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Hello");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<uint> RequestNameAsync(string arg0, uint arg1)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "RequestName", "su");
			writer.WriteString(arg0);
			writer.WriteUInt32(arg1);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<uint> ReleaseNameAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ReleaseName", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<uint> StartServiceByNameAsync(string arg0, uint arg1)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "StartServiceByName", "su");
			writer.WriteString(arg0);
			writer.WriteUInt32(arg1);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task UpdateActivationEnvironmentAsync(Dictionary<string, string> arg0)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "UpdateActivationEnvironment", "a{ss}");
			writer.WriteDictionary_aess(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> NameHasOwnerAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "NameHasOwner", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string[]> ListNamesAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ListNames");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string[]> ListActivatableNamesAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ListActivatableNames");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task AddMatchAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "AddMatch", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task RemoveMatchAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "RemoveMatch", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetNameOwnerAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetNameOwner", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string[]> ListQueuedOwnersAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ListQueuedOwners", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<uint> GetConnectionUnixUserAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetConnectionUnixUser", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<uint> GetConnectionUnixProcessIDAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetConnectionUnixProcessID", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<byte[]> GetAdtAuditSessionDataAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ay);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetAdtAuditSessionData", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<byte[]> GetConnectionSELinuxSecurityContextAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ay);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetConnectionSELinuxSecurityContext", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetConnectionAppArmorSecurityContextAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetConnectionAppArmorSecurityContext", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task ReloadConfigAsync()
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ReloadConfig");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetIdAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetId");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<Dictionary<string, DBusVariantItem>> GetConnectionCredentialsAsync(string arg0)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_aesv);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetConnectionCredentials", "s");
			writer.WriteString(arg0);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public ValueTask<IDisposable> WatchNameOwnerChangedAsync(Action<Exception?, (string Item1, string Item2, string Item3)> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NameOwnerChanged",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_sss, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNameLostAsync(Action<Exception?, string> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NameLost",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_s, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNameAcquiredAsync(Action<Exception?, string> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NameAcquired",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_s, handler, emitOnCapturedContext);
	}

	public Task<string[]> GetFeaturesPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Features");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string[]> GetInterfacesPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Interfaces");
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
				case "Features":
					reader.ReadSignature("as");
					props.Features = reader.ReadArray_as();
					changed?.Add("Features");
					break;
				case "Interfaces":
					reader.ReadSignature("as");
					props.Interfaces = reader.ReadArray_as();
					changed?.Add("Interfaces");
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
		public string[] Features { get; set; }
		public string[] Interfaces { get; set; }
	}
}
