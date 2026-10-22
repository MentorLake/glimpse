namespace Glimpse.Services.Clipboard;

public record ClipboardConfiguration
{
	public static string ConfigKey => "Clipboard";

	public bool Enabled { get; set; } = true;
	public int MaxEntries { get; set; } = 50;

	public static readonly ClipboardConfiguration Empty = new();

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(ClipboardConfiguration), ClipboardSerializationContext.Instance)?.AsObject();
	}

	public static ClipboardConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(ClipboardConfiguration), ClipboardSerializationContext.Instance) as ClipboardConfiguration;
	}
}
