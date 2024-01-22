using System.Collections.Immutable;
using System.Text.Json;

namespace Glimpse.Taskbar;

public record TaskbarConfiguration
{
	public const string ConfigKey = "Taskbar";
	public static readonly TaskbarConfiguration Empty = new();

	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;

	public JsonElement ToJsonElement()
	{
		return JsonSerializer.SerializeToElement(this, typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance);
	}

	public static TaskbarConfiguration From(JsonElement element)
	{
		return element.Deserialize(typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance) as TaskbarConfiguration;
	}
}
