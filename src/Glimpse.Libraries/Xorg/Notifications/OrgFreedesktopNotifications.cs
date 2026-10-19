using Glimpse.Libraries.DBus;
using Glimpse.Libraries.DBus.Core;
using Glimpse.Libraries.Images;

#pragma warning disable
namespace Glimpse.Libraries.Xorg.Notifications;

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
	public GdkPixbufHandle Image { get; set; }
	public string ImagePath { get; set; }
	public bool Resident { get; set; }
	public string SoundFile { get; set; }
	public string SoundName { get; set; }
	public bool SuppressSound { get; set; }
	public bool Transient { get; set; }
	public DateTime CreationDate { get; set; }
}

public class OrgFreedesktopNotifications(DBusConnections dBusConnections) : IPathMethodHandler
{
	private static uint s_idCounter = 1;
	private readonly SynchronizationContext? _synchronizationContext;
	private DBusConnection DBusConnection => dBusConnections.Session;
	public string Path => "/org/freedesktop/Notifications";
	public bool HandlesChildPaths => false;
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
					case ("GetCapabilities", "" or null):
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
							Dictionary<string, VariantValue> hints;
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

	private GdkPixbufHandle ParseImageData(VariantValue variantItem)
	{
		var imageStruct = variantItem.GetVariantValue();
		var width = imageStruct.GetItem(0).GetInt32();
		var height = imageStruct.GetItem(1).GetInt32();
		var rowStride = imageStruct.GetItem(2).GetInt32();
		var hasAlpha = imageStruct.GetItem(3).GetBool();
		var bitsPerSample = imageStruct.GetItem(4).GetInt32();
		var buffer = imageStruct.GetItem(6).GetArray<byte>();

		using var fullSizeImage = GdkPixbufFactory.From(buffer, buffer.Length, hasAlpha, bitsPerSample, width, height, rowStride);
		return fullSizeImage.ScaleToFit(34, 34);
	}

	private ValueTask<uint> OnNotifyAsync(string app_name, uint replaces_id, string app_icon, string summary, string body, string[] actions, Dictionary<string, VariantValue> hints, int expire_timeout)
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
			ActionIcons = hints.TryGetValue("action-icons", out var actionIconsHint) && actionIconsHint.GetBool(),
			Category = hints.TryGetValue("category", out var categoryHint) ? categoryHint.GetString() : "",
			DesktopEntry = hints.TryGetValue("desktop-entry", out var desktopEntry) ? desktopEntry.GetString() : "",
			Image = hints.TryGetValue("image-data", out var imageDataHint) ? ParseImageData(imageDataHint)
				: hints.TryGetValue("image_data", out var imageDataHintDeprecated) ? ParseImageData(imageDataHintDeprecated)
				: hints.TryGetValue("icon_data", out var iconDataHint) ? ParseImageData(iconDataHint)
				: null,
			ImagePath = hints.TryGetValue("image-path", out var imagePath) ? imagePath.GetString()
				: hints.TryGetValue("image_path", out var imagePathDeprecated) ? imagePathDeprecated.GetString()
				: "",
			Resident = hints.TryGetValue("resident", out var resident) && resident.GetBool(),
			SoundFile = hints.TryGetValue("sound-file", out var soundFile) ? soundFile.GetString() : "",
			SoundName = hints.TryGetValue("sound-name", out var soundName) ? soundName.GetString() : "",
			SuppressSound = hints.TryGetValue("suppress-sound", out var suppressSound) && suppressSound.GetBool(),
			Transient = hints.TryGetValue("transient", out var transient) && transient.GetBool(),
			X = hints.TryGetValue("x", out var x) ? x.GetInt32() : null,
			Y = hints.TryGetValue("y", out var y) ? y.GetInt32() : null,
			Urgency = hints.TryGetValue("urgency", out var urgencyHint) ? (NotificationUrgency) urgencyHint.GetByte() : NotificationUrgency.Normal,
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
		var writer = DBusConnection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.freedesktop.Notifications", "NotificationClosed", "uu");
		writer.WriteUInt32(id);
		writer.WriteUInt32(reason);
		DBusConnection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}

	public void EmitActionInvoked(uint id, string action_key)
	{
		var writer = DBusConnection.GetMessageWriter();
		writer.WriteSignalHeader(null, Path, "org.freedesktop.Notifications", "ActionInvoked", "us");
		writer.WriteUInt32(id);
		writer.WriteString(action_key);
		DBusConnection.TrySendMessage(writer.CreateMessage());
		writer.Dispose();
	}
}
