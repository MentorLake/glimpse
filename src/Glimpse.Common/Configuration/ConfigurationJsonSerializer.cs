using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Glimpse.Common.Configuration;

[JsonSerializable(typeof(Dictionary<string, JsonObject>))]
internal partial class ConfigurationSerializationContext : JsonSerializerContext
{
	public static ConfigurationSerializationContext Instance { get; } = new (
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
