using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.Services.SystemTray;

[JsonSerializable(typeof(SystemTrayConfiguration))]
internal partial class SystemTraySerializationContext : JsonSerializerContext
{
	public static SystemTraySerializationContext Instance { get; } = new (
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
