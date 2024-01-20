using System.Reactive.Linq;
using Glimpse.Freedesktop.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Freedesktop.DBus.Interfaces;

public class ComCanonicalDbusmenu
{
	public const string Interface = "com.canonical.dbusmenu";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public ComCanonicalDbusmenu(Connection connection, string destination, string path)
	{
		_connection = connection;
		_destination = destination;
		_path = path;

		LayoutUpdated = connection
			.WatchSignal(
				new MatchRule { Type = MessageType.Signal, Sender = destination, Path = path, Member = "LayoutUpdated", Interface = Interface },
				ReaderExtensions.ReadMessage_ui)
			.Select(x => Observable.FromAsync(() => GetLayoutAsync(x.parent, -1, Array.Empty<string>())))
			.Concat();
	}

	public IObservable<(uint revision, (int, Dictionary<string, DBusVariantItem>, DBusVariantItem[]) layout)> LayoutUpdated { get; }

	public Task<(uint revision, (int, Dictionary<string, DBusVariantItem>, DBusVariantItem[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_uriaesvavz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetLayout", "iias");
			writer.WriteInt32(parentId);
			writer.WriteInt32(recursionDepth);
			writer.WriteArray_as(propertyNames);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(int, Dictionary<string, DBusVariantItem>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ariaesvz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetGroupProperties", "aias");
			writer.WriteArray_ai(ids);
			writer.WriteArray_as(propertyNames);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<DBusVariantItem> GetPropertyAsync(int id, string name)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_v);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "GetProperty", "is");
			writer.WriteInt32(id);
			writer.WriteString(name);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task EventAsync(int id, string eventId, DBusVariantItem data, uint timestamp)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Event", "isvu");
			writer.WriteInt32(id);
			writer.WriteString(eventId);
			writer.WriteDBusVariant(data);
			writer.WriteUInt32(timestamp);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<int[]> EventGroupAsync((int, string, DBusVariantItem, uint)[] events)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ai);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "EventGroup", "a(isvu)");
			writer.WriteArray_arisvuz(events);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> AboutToShowAsync(int id)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "AboutToShow", "i");
			writer.WriteInt32(id);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(int[] updatesNeeded, int[] idErrors)> AboutToShowGroupAsync(int[] ids)
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_aiai);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "AboutToShowGroup", "ai");
			writer.WriteArray_ai(ids);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public ValueTask<IDisposable> WatchItemsPropertiesUpdatedAsync(Action<Exception?, ((int, Dictionary<string, DBusVariantItem>)[] updatedProps, (int, string[])[] removedProps)> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "ItemsPropertiesUpdated",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_ariaesvzariasz, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchLayoutUpdatedAsync(Action<Exception?, (uint revision, int parent)> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "LayoutUpdated",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_ui, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchItemActivationRequestedAsync(Action<Exception?, (int id, uint timestamp)> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "ItemActivationRequested",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_iu, handler, emitOnCapturedContext);
	}

	public Task<uint> GetVersionPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_u);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Version");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetTextDirectionPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("TextDirection");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetStatusPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Status");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string[]> GetIconThemePathPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_as);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("IconThemePath");
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
				case "Version":
					reader.ReadSignature("u");
					props.Version = reader.ReadUInt32();
					changed?.Add("Version");
					break;
				case "TextDirection":
					reader.ReadSignature("s");
					props.TextDirection = reader.ReadString();
					changed?.Add("TextDirection");
					break;
				case "Status":
					reader.ReadSignature("s");
					props.Status = reader.ReadString();
					changed?.Add("Status");
					break;
				case "IconThemePath":
					reader.ReadSignature("as");
					props.IconThemePath = reader.ReadArray_as();
					changed?.Add("IconThemePath");
					break;
			}
		}

		return props;
	}

	public class Properties
	{
		public uint Version { get; set; }
		public string TextDirection { get; set; }
		public string Status { get; set; }
		public string[] IconThemePath { get; set; }
	}
}
