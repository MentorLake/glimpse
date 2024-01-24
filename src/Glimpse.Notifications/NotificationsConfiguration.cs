using System.Collections.Immutable;
using System.Text.Json;
using Glimpse.Freedesktop.Notifications;

namespace Glimpse.Configuration;

public record NotificationsConfiguration
{
	public ImmutableList<NotificationApplicationConfig> Applications { get; set; } = ImmutableList<NotificationApplicationConfig>.Empty;
	public static string ConfigKey { get; set; } = "Notifications";

	public static readonly NotificationsConfiguration Empty = new();

	public JsonElement ToJsonElement()
	{
		return JsonSerializer.SerializeToElement(this, typeof(NotificationsConfiguration), NotificationsJsonSerializer.Instance);
	}

	public static NotificationsConfiguration From(JsonElement element)
	{
		return element.Deserialize(typeof(NotificationsConfiguration), NotificationsJsonSerializer.Instance) as NotificationsConfiguration;
	}
}

public record NotificationApplicationConfig
{
	public string Name { get; set; }
	public bool ShowPopupBubbles { get; set; }
	public bool ShowInHistory { get; set; }
}
