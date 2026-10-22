namespace Glimpse.Services.Clipboard;

[JsonSerializable(typeof(ClipboardHistory))]
[JsonSerializable(typeof(ClipboardConfiguration))]
internal partial class ClipboardSerializationContext : JsonSerializerContext
{
	public static ClipboardSerializationContext Instance { get; } = new(
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
