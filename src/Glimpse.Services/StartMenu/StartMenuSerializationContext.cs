namespace Glimpse.Services.StartMenu;

[JsonSerializable(typeof(StartMenuConfiguration))]
internal partial class StartMenuSerializationContext : JsonSerializerContext
{
	public static StartMenuSerializationContext Instance { get; } = new (
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
