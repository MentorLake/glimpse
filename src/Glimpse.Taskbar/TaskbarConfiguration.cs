using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Glimpse.Taskbar;

public record TaskbarConfiguration
{
	public const string ConfigKey = "Taskbar";
	public static readonly TaskbarConfiguration Empty = new();

	public string TaskManagerCommand { get; set; } = "";
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance)?.AsObject();
	}

	public static TaskbarConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance) as TaskbarConfiguration;
	}
}
