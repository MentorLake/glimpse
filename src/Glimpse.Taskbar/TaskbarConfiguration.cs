using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using Glimpse.Common.Configuration;

namespace Glimpse.Taskbar;

internal record ContextMenuItem
{
	public string DisplayText { get; set; }
	public string Executable { get; set; }
	public string Arguments { get; set; } = "";
}

internal record TaskbarConfiguration
{
	public const string ConfigKey = "Taskbar";
	public static readonly TaskbarConfiguration Empty = new();

	public string TaskManagerCommand { get; set; } = "";
	public string StartMenuLaunchIconName { get; set; } = "start-here";
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;

	public ImmutableList<ContextMenuItem> ContextMenu { get; set; } = ImmutableList.Create<ContextMenuItem>(
		new() { DisplayText = "Terminal", Executable = "x-terminal-emulator" },
		new() { DisplayText = "Display", Executable = "xfce4-display-settings" },
		new() { DisplayText = "Network Connections", Executable = "nm-connection-editor" },
		new() { DisplayText = "Session & Startup", Executable = "xfce4-settings-manager", Arguments = "-d xfce-session-settings" },
		new() { DisplayText = "separator" },
		new() { DisplayText = "Edit Glimpse config", Executable = "xdg-open", Arguments = ConfigurationService.FilePath },
		new() { DisplayText = "System Settings", Executable = "xfce4-settings-manager" },
		new() { DisplayText = "separator" },
		new() { DisplayText = "Shutdown or sign out", Executable = "xfce4-session-logout" });

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance)?.AsObject();
	}

	public static TaskbarConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance) as TaskbarConfiguration;
	}
}
