namespace Glimpse.Services.Notifications;

[JsonSerializable(typeof(NotificationHistory))]
[JsonSerializable(typeof(NotificationsConfiguration))]
internal partial class NotificationsJsonSerializer : JsonSerializerContext
{
	public static NotificationsJsonSerializer Instance { get; } = new(
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
