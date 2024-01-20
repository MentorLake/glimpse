using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.Freedesktop.Notifications;

[JsonSerializable(typeof(NotificationHistory))]
internal partial class NotificationsJsonSerializer : JsonSerializerContext
{
	public static NotificationsJsonSerializer Instance { get; } = new(
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
