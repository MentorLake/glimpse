using System.Collections.Immutable;
using System.Text.Json;

namespace Glimpse.StartMenu;

public record StartMenuLaunchIconContextMenuItem
{
	public string DisplayText { get; set; }
	public string Executable { get; set; }
	public string Arguments { get; set; } = "";
}

public record StartMenuConfiguration
{
	public string StartMenuLaunchIconName { get; set; } = "start-here";
	public string PowerButtonCommand { get; set; } = "xfce4-session-logout";
	public string SettingsButtonCommand { get; set; } = "xfce4-settings-manager";
	public string UserSettingsCommand { get; set; } = "mugshot";
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;

	public ImmutableList<StartMenuLaunchIconContextMenuItem> StartMenuLaunchIconContextMenu { get; set;  } = ImmutableList.Create<StartMenuLaunchIconContextMenuItem>(
		new () { DisplayText = "Terminal", Executable = "xfce4-terminal" },
		new () { DisplayText = "Display", Executable = "xfce4-display-settings" },
		new () { DisplayText = "Gaming Mouse Settings", Executable = "piper" },
		new () { DisplayText = "CPU Power Mode", Executable = "cpupower-gui" },
		new () { DisplayText = "Hardware Information", Executable = "hardinfo" },
		new () { DisplayText = "Network Connections", Executable = "nm-connection-editor" },
		new () { DisplayText = "Session & Startup", Executable = "xfce4-settings-manager", Arguments = "-d xfce-session-settings" });

	public static JsonElement EmptyJson = JsonSerializer.SerializeToElement(new StartMenuConfiguration(), typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance);
}