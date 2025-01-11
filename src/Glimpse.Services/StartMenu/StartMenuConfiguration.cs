using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Glimpse.Services.StartMenu;

public record StartMenuConfiguration
{
	public const string ConfigKey = "StartMenu";
	public static StartMenuConfiguration Empty = new();

	public string PowerButtonCommand { get; set; } = "xfce4-session-logout";
	public string SettingsButtonCommand { get; set; } = "xfce4-settings-manager";
	public string UserSettingsCommand { get; set; } = "mugshot";
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance)?.AsObject();
	}

	public static StartMenuConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance) as StartMenuConfiguration;
	}
}
