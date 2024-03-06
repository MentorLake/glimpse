using System.Reactive.Subjects;
using Glimpse.Common.DBus;
using Glimpse.Common.DBus.Core;
using Glimpse.Common.Images;
using MentorLake.Redux;
using Tmds.DBus.Protocol;

#pragma warning disable
namespace Glimpse.Notifications;

public enum NotificationUrgency : byte
{
	Low = 0,
	Normal = 1,
	Critical = 2
}

public record FreedesktopNotification : IKeyed<uint>
{
	public string AppName { get; set; }
	public uint ReplacesId { get; set; }
	public string AppIcon { get; set; }
	public string Summary { get; set; }
	public string Body { get; set; }
	public string[] Actions { get; set; } = Array.Empty<string>();
	public TimeSpan Duration { get; set; }
	public uint Id { get; set; }
	public NotificationUrgency Urgency { get; set; }
	public string Category { get; set; }
	public string DesktopEntry { get; set; }
	public int? X { get; set; }
	public int? Y { get; set; }
	public bool ActionIcons { get; set; }
	public IGlimpseImage Image { get; set; }
	public string ImagePath { get; set; }
	public bool Resident { get; set; }
	public string SoundFile { get; set; }
	public string SoundName { get; set; }
	public bool SuppressSound { get; set; }
	public bool Transient { get; set; }
	public DateTime CreationDate { get; set; }
}

public class OrgFreedesktopNotifications(DBusConnections dBusConnections) : IMethodHandler
{
	private static uint s_idCounter = 1;
	private readonly SynchronizationContext? _synchronizationContext;
	private Connection Connection => dBusConnections.Session;
	public string Path => "/org/freedesktop/Notifications";
	public bool RunMethodHandlerSynchronously(Message message) => true;
	private readonly Subject<FreedesktopNotification> _notificationSubject = new();
	public IObservable<FreedesktopNotification> Notifications => _notificationSubject;

	private readonly Subject<uint> _closeNotificationSubject = new();
	public IObservable<uint> CloseNotificationRequested => _closeNotificationSubject;

	public async ValueTask HandleMethodAsync(MethodContext context)
	{
		switch (context.Request.InterfaceAsString)
		{
			case "org.freedesktop.Notifications":
				switch (context.Request.MemberAsString, context.Request.SignatureAsString)
				{
					case ("Introspect", "" or null):
						{
							string[] ret;
							if (_synchronizationContext is not null)
							{
								TaskCompletionSource<string[]> tsc = new();
								_synchronizationContext.Post(async _ =>
								{
									try
									{
										var ret1 = await OnGetCapabilitiesAsync();
										tsc.SetResult(ret1);
									}
									catch (Exception e)
									{
										tsc.SetException(e);
									}
								}, null);
								ret = await tsc.Task;
							}
							else
							{
								ret = await OnGetCapabilitiesAsync();
							}

							Reply();

							void Reply()
							{
								var writer = context.CreateReplyWriter("as");
								writer.WriteArray_as(ret);
								context.Reply(writer.CreateMessage());
								writer.Dispose();
							}

							break;
						}

					case ("GetServerInformation", "" or null):
						{
							var ret = await OnGetServerInformationAsync();
							Reply();

							void Reply()
							{
								var writer = context.CreateReplyWriter("ssss");
								writer.WriteString(ret.name);
								writer.WriteString(ret.vendor);
								writer.WriteString(ret.version);
								writer.WriteString(ret.spec_version);
								context.Reply(writer.CreateMessage());
								writer.Dispose();
							}

							break;
						}

					case ("Notify", "susssasa{sv}i"):
						{
							string app_name;
							uint replaces_id;
							string app_icon;
							string summary;
							string body;
							string[] actions;
							Dictionary<string, DBusVariantItem> hints;
							int expire_timeout;
							ReadParameters();

							void ReadParameters()
							{
								var reader = context.Request.GetBodyReader();
								app_name = reader.ReadString();
								replaces_id = reader.ReadUInt32();
								app_icon = reader.ReadString();
								summary = reader.ReadString();
								body = reader.ReadString();
								actions = reader.ReadArray_as();
								hints = reader.ReadDictionary_aesv();
								expire_timeout = reader.ReadInt32();
							}

							uint ret;
							if (_synchronizationContext is not null)
							{
								TaskCompletionSource<uint> tsc = new();
								_synchronizationContext.Post(async _ =>
								{
									try
									{
										var ret1 = await OnNotifyAsync(app_name, replaces_id, app_icon, summary, body, actions, hints, expire_timeout);
										tsc.SetResult(ret1);
									}
									catch (Exception e)
									{
										tsc.SetException(e);
									}
								}, null);
								ret = await tsc.Task;
							}
							else
							{
								ret = await OnNotifyAsync(app_name, replaces_id, app_icon, summary, body, actions, hints, expire_timeout);
							}

							Reply();

							void Reply()
							{
								var writer = context.CreateReplyWriter("u");
								writer.WriteUInt32(ret);
								context.Reply(writer.CreateMessage());
								writer.Dispose();
							}

							break;
						}

					case ("CloseNotification", "u"):
						{
							uint id;
							ReadParameters();

							void ReadParameters()
							{
								var reader = context.Request.GetBodyReader();
								id = reader.ReadUInt32();
							}

							if (_synchronizationContext is not null)
							{
								TaskCompletionSource<bool> tsc = new();
								_synchronizationContext.Post(async _ =>
								{
									try
									{
										await OnCloseNotificationAsync(id);
										tsc.SetResult(true);
									}
									catch (Exception e)
									{
										tsc.SetException(e);
									}
								}, null);
								await tsc.Task;
							}
							else
							{
								await OnCloseNotificationAsync(id);
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
								writer.WriteString("<!DOCTYPE node PUBLIC \"-//freedesktop//DTD D-BUS Object Introspection 1.0//EN\"\n\"http://www.freedesktop.org/standards/dbus/1.0/introspect.dtd\"><node xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n  <interface name=\"org.freedesktop.Notifications\">\n    <method name=\"GetCapabilities\">\n      <arg name=\"capabilities\" type=\"as\" direction=\"out\" />\n    </method>\n    <method name=\"Notify\">\n      <arg name=\"app_name\" type=\"s\" direction=\"in\" />\n      <arg name=\"replaces_id\" type=\"u\" direction=\"in\" />\n      <arg name=\"app_icon\" type=\"s\" direction=\"in\" />\n      <arg name=\"summary\" type=\"s\" direction=\"in\" />\n      <arg name=\"body\" type=\"s\" direction=\"in\" />\n      <arg name=\"actions\" type=\"as\" direction=\"in\" />\n      <arg name=\"hints\" type=\"a{sv}\" direction=\"in\" />\n      <arg name=\"expire_timeout\" type=\"i\" direction=\"in\" />\n      <arg name=\"id\" type=\"u\" direction=\"out\" />\n    </method>\n    <method name=\"CloseNotification\">\n      <arg name=\"id\" type=\"u\" direction=\"in\" />\n    </method>\n    <method name=\"GetServerInformation\">\n      <arg name=\"name\" type=\"s\" direction=\"out\" />\n      <arg name=\"vendor\" type=\"s\" direction=\"out\" />\n      <arg name=\"version\" type=\"s\" direction=\"out\" />\n      <arg name=\"spec_version\" type=\"s\" direction=\"out\" />\n    </method>\n    <signal name=\"NotificationClosed\">\n      <arg name=\"id\" type=\"u\" />\n      <arg name=\"reason\" type=\"u\" />\n    </signal>\n    <signal name=\"ActionInvoked\">\n      <arg name=\"id\" type=\"u\" />\n      <arg name=\"action_key\" type=\"s\" />\n    </signal>\n  </interface>\n</node>");
								context.Reply(writer.CreateMessage());
							}

							break;
						}
				}

				break;
		}
	}

	private ValueTask<string[]> OnGetCapabilitiesAsync()
	{
		return ValueTask.FromResult(new[] { "actions", "body" });
	}

	private IGlimpseImage ParseImageData(DBusVariantItem variantItem)
	{
		var imageStruct = variantItem.Value as DBusStructItem;
		var width = imageStruct[0] as DBusInt32Item;
		var height = imageStruct[1] as DBusInt32Item;
		var rowStride = imageStruct[2] as DBusInt32Item;
		var hasAlpha = imageStruct[3] as DBusBoolItem;
		var bitsPerSample = imageStruct[4] as DBusInt32Item;
		var channels = imageStruct[5] as DBusInt32Item;
		var data = imageStruct[6] as DBusArrayItem;
		var dataArray = data.Select(i => i as DBusByteItem).Select(i => i.Value).ToArray();
		return GlimpseImageFactory.From(dataArray, hasAlpha.Value, bitsPerSample.Value, width.Value, height.Value, rowStride.Value);
	}

	private ValueTask<uint> OnNotifyAsync(string app_name, uint replaces_id, string app_icon, string summary, string body, string[] actions, Dictionary<string, DBusVariantItem> hints, int expire_timeout)
	{
		var notification = new FreedesktopNotification()
		{
			Id = s_idCounter,
			AppName = app_name,
			ReplacesId = replaces_id,
			AppIcon = app_icon,
			Summary = summary,
			Body = body,
			Actions = actions,
			Duration = expire_timeout <= 0 ? TimeSpan.FromMilliseconds(3000) : TimeSpan.FromMilliseconds(expire_timeout),
			ActionIcons = hints.TryGetValue("action-icons", out var actionIconsHint) && (actionIconsHint.Value as DBusBoolItem).Value,
			Category = hints.TryGetValue("category", out var categoryHint) ? (categoryHint.Value as DBusStringItem).Value : "",
			DesktopEntry = hints.TryGetValue("desktop-entry", out var desktopEntry) ? (desktopEntry.Value as DBusStringItem).Value : "",
			Image = hints.TryGetValue("image-data", out var imageDataHint) ? ParseImageData(imageDataHint)
				: hints.TryGetValue("image_data", out var imageDataHintDeprecated) ? ParseImageData(imageDataHintDeprecated)
				: hints.TryGetValue("icon_data", out var iconDataHint) ? ParseImageData(iconDataHint)
				: null,
			ImagePath = hints.TryGetValue("image-path", out var imagePath) ? (imagePath.Value as DBusStringItem).Value
				: hints.TryGetValue("image_path", out var imagePathDeprecated) ? (imagePathDeprecated.Value as DBusStringItem).Value
				: "",
			Resident = hints.TryGetValue("resident", out var resident) && (imagePath.Value as DBusBoolItem).Value,
			SoundFile = hints.TryGetValue("sound-file", out var soundFile) ? (soundFile.Value as DBusStringItem).Value : "",
			SoundName = hints.TryGetValue("sound-name", out var soundName) ? (soundName.Value as DBusStringItem).Value : "",
			SuppressSound = hints.TryGetValue("suppress-sound", out var suppressSound) && (suppressSound.Value as DBusBoolItem).Value,
			Transient = hints.TryGetValue("transient", out var transient) && (transient.Value as DBusBoolItem).Value,
			X = hints.TryGetValue("x", out var x) ? (x.Value as DBusInt32Item).Value : null,
			Y = hints.TryGetValue("y", out var y) ? (y.Value as DBusInt32Item).Value : null,
			Urgency = hints.TryGetValue("urgency", out var urgencyHint) ? (NotificationUrgency) ((urgencyHint.Value as DBusByteItem)?.Value ?? 1) : NotificationUrgency.Normal,
			CreationDate = DateTime.UtcNow
		};

		_notificationSubject.OnNext(notification);
		return ValueTask.FromResult(s_idCounter++);
	}

	private ValueTask OnCloseNotificationAsync(uint id)
	{
		_closeNotificationSubject.OnNext(id);
		return ValueTask.CompletedTask;
	}

	private ValueTask<(string name, string vendor, string version, string spec_version)> OnGetServerInformationAsync()
	{
		return ValueTask.FromResult<(string name, string vendor, string version, string spec_version)>(("glimpse", "glimpse", "1", "1.2"));
	}

	public void EmitNotificationClosed(uint id, uint reason)
	{
		var writer = Connection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.freedesktop.Notifications", "NotificationClosed", "uu");
		writer.WriteUInt32(id);
		writer.WriteUInt32(reason);
		Connection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}

	public void EmitActionInvoked(uint id, string action_key)
	{
		var writer = Connection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.freedesktop.Notifications", "ActionInvoked", "us");
		writer.WriteUInt32(id);
		writer.WriteString(action_key);
		Connection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}
}
