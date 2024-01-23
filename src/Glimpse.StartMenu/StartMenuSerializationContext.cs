using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.StartMenu;

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
