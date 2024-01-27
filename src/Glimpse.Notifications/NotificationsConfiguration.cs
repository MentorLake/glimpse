using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Glimpse.Notifications;

public record NotificationsConfiguration
{
	public ImmutableList<NotificationApplicationConfig> Applications { get; set; } = ImmutableList<NotificationApplicationConfig>.Empty;
	public static string ConfigKey { get; set; } = "Notifications";

	public static readonly NotificationsConfiguration Empty = new();

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(NotificationsConfiguration), NotificationsJsonSerializer.Instance)?.AsObject();
	}

	public static NotificationsConfiguration From(JsonObject element)
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
