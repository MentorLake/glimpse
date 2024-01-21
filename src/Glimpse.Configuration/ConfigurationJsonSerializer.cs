using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.Configuration;

[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(ConfigurationFile))]
internal partial class ConfigurationSerializationContext : JsonSerializerContext
{
	public static ConfigurationSerializationContext Instance { get; } = new (
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
