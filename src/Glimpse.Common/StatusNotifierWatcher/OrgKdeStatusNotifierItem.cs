using System.Reactive.Linq;
using System.Text;
using Glimpse.Common.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.StatusNotifierWatcher;

public class OrgKdeStatusNotifierItem
{
	public const string Interface = "org.kde.StatusNotifierItem";
	private readonly Connection _connection;
	private readonly string _destination;
	private readonly string _path;

	public OrgKdeStatusNotifierItem(Connection connection, string destination, string path)
	{
		_connection = connection;
		_destination = destination;
		_path = path;

		var iconUpdated = connection.WatchSignal(
			new MatchRule
			{
				Type = MessageType.Signal,
				Sender = _destination,
				Path = _path,
				Member = "NewIcon",
				Interface = Interface
			});

		PropertyChanged = connection.WatchSignal(
			new MatchRule
			{
				Type = MessageType.Signal,
				Sender = destination,
				Path = path,
				Member = "PropertiesChanged",
				Interface = "org.freedesktop.DBus.Properties",
				Arg0 = Interface
			})
			.Merge(iconUpdated)
			.Select(_ => Observable.FromAsync(GetAllPropertiesAsync))
			.Concat();
	}

	public IObservable<Properties> PropertyChanged { get; }

	public Task ContextMenuAsync(int x, int y)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "ContextMenu", "ii");
			writer.WriteInt32(x);
			writer.WriteInt32(y);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task ActivateAsync(int x, int y)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Activate", "ii");
			writer.WriteInt32(x);
			writer.WriteInt32(y);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task SecondaryActivateAsync(int x, int y)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "SecondaryActivate", "ii");
			writer.WriteInt32(x);
			writer.WriteInt32(y);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task ScrollAsync(int delta, string orientation)
	{
		return _connection.CallMethodAsync(CreateMessage());

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, Interface, "Scroll", "is");
			writer.WriteInt32(delta);
			writer.WriteString(orientation);
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public ValueTask<IDisposable> WatchNewTitleAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewTitle",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNewIconAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewIcon",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNewAttentionIconAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewAttentionIcon",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNewOverlayIconAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewOverlayIcon",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNewMenuAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewMenu",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNewToolTipAsync(Action<Exception?> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewToolTip",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, handler, emitOnCapturedContext);
	}

	public ValueTask<IDisposable> WatchNewStatusAsync(Action<Exception?, string> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = _destination,
			Path = _path,
			Member = "NewStatus",
			Interface = Interface
		};
		return SignalHelper.WatchSignalAsync(_connection, rule, ReaderExtensions.ReadMessage_s, handler, emitOnCapturedContext);
	}

	public Task<string> GetCategoryPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Category");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetIdPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Id");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetTitlePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Title");
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

	public Task<int> GetWindowIdPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_i);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("WindowId");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetIconThemePathPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

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

	public Task<ObjectPath> GetMenuPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_o);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("Menu");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<bool> GetItemIsMenuPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_b);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("ItemIsMenu");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetIconNamePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("IconName");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(int, int, byte[])[]> GetIconPixmapPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ariiayz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("IconPixmap");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetOverlayIconNamePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("OverlayIconName");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(int, int, byte[])[]> GetOverlayIconPixmapPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ariiayz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("OverlayIconPixmap");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetAttentionIconNamePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("AttentionIconName");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(int, int, byte[])[]> GetAttentionIconPixmapPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_ariiayz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("AttentionIconPixmap");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<string> GetAttentionMovieNamePropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_s);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("AttentionMovieName");
			var message = writer.CreateMessage();
			writer.Dispose();
			return message;
		}
	}

	public Task<(string, (int, int, byte[])[], string, string)> GetToolTipPropertyAsync()
	{
		return _connection.CallMethodAsync(CreateMessage(), ReaderExtensions.ReadMessage_rsariiayzssz);

		MessageBuffer CreateMessage()
		{
			var writer = _connection.GetMessageWriter();
			writer.WriteMethodCallHeader(_destination, _path, "org.freedesktop.DBus.Properties", "Get", "ss");
			writer.WriteString(Interface);
			writer.WriteString("ToolTip");
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
			var propName = reader.ReadString();
			switch (propName)
			{
				case "Category":
					reader.ReadSignature("s");
					props.Category = reader.ReadString();
					changed?.Add("Category");
					break;
				case "Id":
					reader.ReadSignature("s");
					props.Id = reader.ReadString();
					changed?.Add("Id");
					break;
				case "Title":
					reader.ReadSignature("s");
					props.Title = reader.ReadString();
					changed?.Add("Title");
					break;
				case "Status":
					reader.ReadSignature("s");
					props.Status = reader.ReadString();
					changed?.Add("Status");
					break;
				case "WindowId":
					var windowIdSig = Encoding.ASCII.GetString(reader.ReadSignatureAsSpan().ToArray());
					if (windowIdSig != "u" && windowIdSig != "i") throw new Exception("Invalid window Id signature");
					props.WindowId = reader.ReadInt32();
					changed?.Add("WindowId");
					break;
				case "IconThemePath":
					reader.ReadSignature("s");
					props.IconThemePath = reader.ReadString();
					changed?.Add("IconThemePath");
					break;
				case "Menu":
					var menuSig = Encoding.ASCII.GetString(reader.ReadSignatureAsSpan().ToArray());
					props.Menu = menuSig == "o" ? reader.ReadObjectPath() : reader.ReadString();
					changed?.Add("Menu");
					break;
				case "ItemIsMenu":
					reader.ReadSignature("b");
					props.ItemIsMenu = reader.ReadBool();
					changed?.Add("ItemIsMenu");
					break;
				case "IconName":
					reader.ReadSignature("s");
					props.IconName = reader.ReadString();
					changed?.Add("IconName");
					break;
				case "IconPixmap":
					reader.ReadSignature("a(iiay)");
					props.IconPixmap = reader.ReadArray_ariiayz();
					changed?.Add("IconPixmap");
					break;
				case "OverlayIconName":
					reader.ReadSignature("s");
					props.OverlayIconName = reader.ReadString();
					changed?.Add("OverlayIconName");
					break;
				case "OverlayIconPixmap":
					reader.ReadSignature("a(iiay)");
					props.OverlayIconPixmap = reader.ReadArray_ariiayz();
					changed?.Add("OverlayIconPixmap");
					break;
				case "AttentionIconName":
					reader.ReadSignature("s");
					props.AttentionIconName = reader.ReadString();
					changed?.Add("AttentionIconName");
					break;
				case "AttentionIconPixmap":
					reader.ReadSignature("a(iiay)");
					props.AttentionIconPixmap = reader.ReadArray_ariiayz();
					changed?.Add("AttentionIconPixmap");
					break;
				case "AttentionMovieName":
					reader.ReadSignature("s");
					props.AttentionMovieName = reader.ReadString();
					changed?.Add("AttentionMovieName");
					break;
				case "ToolTip":
					reader.ReadSignature("(sa(iiay)ss)");
					props.ToolTip = reader.ReadStruct_rsariiayzssz();
					changed?.Add("ToolTip");
					break;
				default:
					var sigType = Encoding.ASCII.GetString(reader.ReadSignatureAsSpan().ToArray());
					if (sigType == "s") reader.ReadString();
					else if (sigType == "u") reader.ReadUInt32();
					else throw new Exception($"Unhandled property: {propName} ({sigType})");
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
		public string Category { get; set; }
		public string Id { get; set; }
		public string Title { get; set; }
		public string Status { get; set; }
		public int WindowId { get; set; }
		public string IconThemePath { get; set; }
		public ObjectPath Menu { get; set; }
		public bool ItemIsMenu { get; set; }
		public string IconName { get; set; }
		public (int, int, byte[])[] IconPixmap { get; set; }
		public string OverlayIconName { get; set; }
		public (int, int, byte[])[] OverlayIconPixmap { get; set; }
		public string AttentionIconName { get; set; }
		public (int, int, byte[])[] AttentionIconPixmap { get; set; }
		public string AttentionMovieName { get; set; }
		public (string, (int, int, byte[])[], string, string) ToolTip { get; set; }
	}
}
