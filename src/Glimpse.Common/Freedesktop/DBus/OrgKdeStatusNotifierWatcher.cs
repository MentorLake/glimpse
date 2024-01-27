using System.Reactive.Subjects;
using System.Text;
using Glimpse.Common.Freedesktop.DBus.Core;
using Glimpse.Common.Freedesktop.DBus.Interfaces;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop.DBus;

public class OrgKdeStatusNotifierWatcher(OrgFreedesktopDBus dbusInterface, DBusConnections dBusConnections) : IMethodHandler
{
	private readonly Subject<string> _itemRegistered = new();
	private readonly Subject<string> _itemRemoved = new();

	private Connection Connection { get; } = dBusConnections.Session;
	public string Path { get; } = "/StatusNotifierWatcher";
	public IObservable<string> ItemRegistered => _itemRegistered;
	public IObservable<string> ItemRemoved => _itemRemoved;
	public Properties BackingProperties { get; } = new();

	public void Initialize()
	{
		dbusInterface.NameChanged.Subscribe(t =>
		{
			var matchingItem = BackingProperties.RegisteredStatusNotifierItems.FirstOrDefault(s => s == t.Item1 && string.IsNullOrEmpty(t.Item3));

			if (!string.IsNullOrEmpty(matchingItem))
			{
				BackingProperties.RegisteredStatusNotifierItems = BackingProperties.RegisteredStatusNotifierItems.Where(s => s != matchingItem).ToArray();
				_itemRemoved.OnNext(matchingItem);
				EmitStatusNotifierItemUnregistered(matchingItem);
			}
		});
	}

	public bool RunMethodHandlerSynchronously(Message message) => true;

	public void RegisterStatusNotifierHostAsync(string service)
	{
		BackingProperties.IsStatusNotifierHostRegistered = true;
		EmitStatusNotifierHostRegistered();
	}

	private ValueTask OnRegisterStatusNotifierItemAsync(string sender, string service)
	{
		BackingProperties.RegisteredStatusNotifierItems = BackingProperties.RegisteredStatusNotifierItems.Concat(new[] { sender }).ToArray();
		EmitStatusNotifierItemRegistered(sender);
		_itemRegistered.OnNext(sender);
		return ValueTask.CompletedTask;
	}

	public async ValueTask HandleMethodAsync(MethodContext context)
	{
		switch (context.Request.InterfaceAsString)
		{
			case "org.kde.StatusNotifierWatcher":
				switch (context.Request.MemberAsString, context.Request.SignatureAsString)
				{
					case ("RegisterStatusNotifierHost", "s"):
						{
							string service;
							ReadParameters();

							void ReadParameters()
							{
								var reader = context.Request.GetBodyReader();
								service = reader.ReadString();
							}

							if (!context.NoReplyExpected)
							{
								Reply();
							}

							void Reply()
							{
								var writer = context.CreateReplyWriter(null !);
								context.Reply(writer.CreateMessage());
								writer.Dispose();
							}

							break;
						}

					case ("RegisterStatusNotifierItem", "s"):
						{
							string service;
							ReadParameters();

							void ReadParameters()
							{
								var reader = context.Request.GetBodyReader();
								service = reader.ReadString();
							}

							await OnRegisterStatusNotifierItemAsync(Encoding.ASCII.GetString(context.Request.Sender), service);

							if (!context.NoReplyExpected)
							{
								Reply();
							}

							void Reply()
							{
								var writer = context.CreateReplyWriter(null !);
								context.Reply(writer.CreateMessage());
								writer.Dispose();
							}

							break;
						}
				}

				break;
			case "org.freedesktop.DBus.Properties":
				switch (context.Request.MemberAsString, context.Request.SignatureAsString)
				{
					case ("Get", "ss"):
						{
							Reply();

							void Reply()
							{
								var reader = context.Request.GetBodyReader();
								reader.ReadString();
								var member = reader.ReadString();
								switch (member)
								{
									case "RegisteredStatusNotifierItems":
										{
											var writer = context.CreateReplyWriter("v");
											writer.WriteDBusVariant(new DBusVariantItem("as", new DBusArrayItem(DBusType.String, BackingProperties.RegisteredStatusNotifierItems.Select(x => new DBusStringItem(x)).ToArray())));
											context.Reply(writer.CreateMessage());
											writer.Dispose();
											break;
										}

									case "IsStatusNotifierHostRegistered":
										{
											var writer = context.CreateReplyWriter("v");
											writer.WriteDBusVariant(new DBusVariantItem("b", new DBusBoolItem(BackingProperties.IsStatusNotifierHostRegistered)));
											context.Reply(writer.CreateMessage());
											writer.Dispose();
											break;
										}

									case "ProtocolVersion":
										{
											var writer = context.CreateReplyWriter("v");
											writer.WriteDBusVariant(new DBusVariantItem("i", new DBusInt32Item(BackingProperties.ProtocolVersion)));
											context.Reply(writer.CreateMessage());
											writer.Dispose();
											break;
										}
								}
							}

							break;
						}

					case ("GetAll", "s"):
						{
							Reply();

							void Reply()
							{
								var writer = context.CreateReplyWriter("a{sv}");
								var dict = new Dictionary<string, DBusVariantItem> { { "RegisteredStatusNotifierItems", new DBusVariantItem("as", new DBusArrayItem(DBusType.String, BackingProperties.RegisteredStatusNotifierItems.Select(x => new DBusStringItem(x)).ToArray())) }, { "IsStatusNotifierHostRegistered", new DBusVariantItem("b", new DBusBoolItem(BackingProperties.IsStatusNotifierHostRegistered)) }, { "ProtocolVersion", new DBusVariantItem("i", new DBusInt32Item(BackingProperties.ProtocolVersion)) } };
								writer.WriteDictionary_aesv(dict);
								context.Reply(writer.CreateMessage());
							}

							break;
						}
				}

				break;
			case "org.freedesktop.DBus.Introspectable":
				switch (context.Request.MemberAsString, context.Request.SignatureAsString)
				{
					case ("Introspect", "" or null):
						{
							Reply();

							void Reply()
							{
								var writer = context.CreateReplyWriter("s");
								writer.WriteString("<!DOCTYPE node PUBLIC \"-//freedesktop//DTD D-BUS Object Introspection 1.0//EN\"\n\"http://www.freedesktop.org/standards/dbus/1.0/introspect.dtd\"><node xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n  <interface name=\"org.kde.StatusNotifierWatcher\">\n    <method name=\"RegisterStatusNotifierHost\">\n      <arg name=\"service\" type=\"s\" direction=\"in\" />\n    </method>\n    <method name=\"RegisterStatusNotifierItem\">\n      <arg name=\"service\" type=\"s\" direction=\"in\" />\n    </method>\n    <signal name=\"StatusNotifierHostRegistered\" />\n    <signal name=\"StatusNotifierItemUnregistered\">\n      <arg type=\"s\" />\n    </signal>\n    <signal name=\"StatusNotifierItemRegistered\">\n      <arg type=\"s\" />\n    </signal>\n    <property name=\"RegisteredStatusNotifierItems\" type=\"as\" access=\"read\" />\n    <property name=\"IsStatusNotifierHostRegistered\" type=\"b\" access=\"read\" />\n    <property name=\"ProtocolVersion\" type=\"i\" access=\"read\" />\n  </interface>\n</node>");
								context.Reply(writer.CreateMessage());
							}

							break;
						}
				}

				break;
		}
	}

	protected void EmitStatusNotifierHostRegistered()
	{
		var writer = Connection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.kde.StatusNotifierWatcher", "StatusNotifierHostRegistered");
		Connection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}

	protected void EmitStatusNotifierItemUnregistered(string arg0)
	{
		var writer = Connection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.kde.StatusNotifierWatcher", "StatusNotifierItemUnregistered", "s");
		writer.WriteString(arg0);
		Connection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}

	protected void EmitStatusNotifierItemRegistered(string arg0)
	{
		var writer = Connection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.kde.StatusNotifierWatcher", "StatusNotifierItemRegistered", "s");
		writer.WriteString(arg0);
		Connection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}

	public class Properties
	{
		public string[] RegisteredStatusNotifierItems { get; set; } = Array.Empty<string>();
		public bool IsStatusNotifierHostRegistered { get; set; }
		public int ProtocolVersion { get; set; }
	}
}
