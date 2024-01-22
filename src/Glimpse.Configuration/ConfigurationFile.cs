using System.Collections.Immutable;

namespace Glimpse.Configuration;

public record ConfigurationFile
{
	public Notifications Notifications { get; set; } = new();
	public string VolumeCommand { get; set; } = "pavucontrol";
	public string TaskManagerCommand { get; set; } = "xfce4-taskmanager";
	public static string FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "glimpse", "config.json");
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
