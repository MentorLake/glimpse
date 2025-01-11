using System.Text.Json;
using System.Text.Json.Nodes;

namespace Glimpse.Services.SystemTray;

public record SystemTrayConfiguration
{
	public const string ConfigKey = "SystemTray";
	public static SystemTrayConfiguration Empty = new();

	public string VolumeCommand { get; set; } = "pavucontrol";

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(SystemTrayConfiguration), SystemTraySerializationContext.Instance)?.AsObject();
	}

	public static SystemTrayConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(SystemTrayConfiguration), SystemTraySerializationContext.Instance) as SystemTrayConfiguration;
	}
}
