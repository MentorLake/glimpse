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
	public string Icon { get; set; } = "";
}

internal record TaskbarConfiguration
{
	public const string ConfigKey = "Taskbar";
	public static readonly TaskbarConfiguration Empty = new();

	public string TaskManagerCommand { get; set; } = "xfce4-taskmanager";
	public string StartMenuLaunchIconName { get; set; } = "";
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;
	public ImmutableList<ContextMenuItem> ContextMenu { get; set; } = ImmutableList<ContextMenuItem>.Empty;

	public JsonObject ToJsonElement()
	{
		return JsonSerializer.SerializeToNode(this, typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance)?.AsObject();
	}

	public static TaskbarConfiguration From(JsonObject element)
	{
		return element.Deserialize(typeof(TaskbarConfiguration), TaskbarSerializationContext.Instance) as TaskbarConfiguration;
	}

	public static TaskbarConfiguration New(string configFilePath)
	{
		return new TaskbarConfiguration()
		{
			TaskManagerCommand = "",
			StartMenuLaunchIconName = "start-here",
			PinnedLaunchers = ImmutableList<string>.Empty,
			ContextMenu = ImmutableList.Create<ContextMenuItem>(
				new() { DisplayText = "Terminal", Executable = "x-terminal-emulator", Icon = "terminal" },
				new() { DisplayText = "Display", Executable = "xfce4-display-settings", Icon = "display" },
				new() { DisplayText = "Network Connections", Executable = "nm-connection-editor", Icon = "network-wireless" },
				new() { DisplayText = "Session & Startup", Executable = "xfce4-settings-manager", Arguments = "-d xfce-session-settings", Icon = "gnome-session" },
				new() { DisplayText = "separator" },
				new() { DisplayText = "Edit Glimpse config", Executable = "xdg-open", Arguments = configFilePath, Icon = "edit" },
				new() { DisplayText = "System Settings", Executable = "xfce4-settings-manager", Icon = "system-settings" },
				new() { DisplayText = "separator" },
				new() { DisplayText = "Shutdown or sign out", Executable = "xfce4-session-logout", Icon = "system-shutdown" })
		};
	}
}
