using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.Taskbar;

[JsonSerializable(typeof(TaskbarConfiguration))]
internal partial class TaskbarSerializationContext : JsonSerializerContext
{
	public static TaskbarSerializationContext Instance { get; } = new (
		new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		});
}
