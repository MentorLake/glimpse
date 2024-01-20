using System.Collections.Immutable;

namespace Glimpse.Configuration;

public record ConfigurationFile
{
	public TaskbarConfiguration Taskbar { get; set; } = new();
	public StartMenuConfiguration StartMenu { get; set; } = new();
	public Notifications Notifications { get; set; } = new();
	public string PowerButtonCommand { get; set; } = "xfce4-session-logout";
	public string SettingsButtonCommand { get; set; } = "xfce4-settings-manager";
	public string UserSettingsCommand { get; set; } = "mugshot";
	public string VolumeCommand { get; set; } = "pavucontrol";
	public string TaskManagerCommand { get; set; } = "xfce4-taskmanager";
	public static string FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "glimpse", "config.json");

	public ImmutableList<StartMenuLaunchIconContextMenuItem> StartMenuLaunchIconContextMenu { get; set;  } = ImmutableList.Create<StartMenuLaunchIconContextMenuItem>(
		new () { DisplayText = "Terminal", Executable = "xfce4-terminal" },
		new () { DisplayText = "Display", Executable = "xfce4-display-settings" },
		new () { DisplayText = "Gaming Mouse Settings", Executable = "piper" },
		new () { DisplayText = "CPU Power Mode", Executable = "cpupower-gui" },
		new () { DisplayText = "Hardware Information", Executable = "hardinfo" },
		new () { DisplayText = "Network Connections", Executable = "nm-connection-editor" },
		new () { DisplayText = "Session & Startup", Executable = "xfce4-settings-manager", Arguments = "-d xfce-session-settings" });
}

public record Notifications
{
	public ImmutableList<NotificationApplicationConfig> Applications { get; set; } = ImmutableList<NotificationApplicationConfig>.Empty;
}

public record NotificationApplicationConfig
{
	public string Name { get; set; }
	public bool ShowPopupBubbles { get; set; }
	public bool ShowInHistory { get; set; }

}

public record StartMenuLaunchIconContextMenuItem
{
	public string DisplayText { get; set; }
	public string Executable { get; set; }
	public string Arguments { get; set; } = "";
}

public record StartMenuConfiguration
{
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;
}

public record TaskbarConfiguration
{
	public string StartMenuLaunchIconName { get; set; } = "start-here";
	public ImmutableList<string> PinnedLaunchers { get; set; } = ImmutableList<string>.Empty;
}
