namespace Glimpse.Configuration;

public record ConfigurationFile
{
	public string VolumeCommand { get; set; } = "pavucontrol";
	public string TaskManagerCommand { get; set; } = "xfce4-taskmanager";
	public static string FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "glimpse", "config.json");
}
