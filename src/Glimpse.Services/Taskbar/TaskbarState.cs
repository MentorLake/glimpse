namespace Glimpse.Services.Taskbar;

internal record TaskbarState
{
	public SlotReferences StoredSlots = new();
	public TaskbarConfiguration Configuration = new();

	public virtual bool Equals(TaskbarState other) => ReferenceEquals(this, other);
}

public record SlotReferences
{
	public ImmutableList<SlotRef> Refs = ImmutableList<SlotRef>.Empty;

	public virtual bool Equals(SlotReferences other)
	{
		return other.Refs.SequenceEqual(Refs);
	}
}

public record SlotRef
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public string PinnedDesktopFileId { get; init; } = "";
	public string ClassHintName { get; init; } = "";
	public string DiscoveredDesktopFileId { get; init; } = "";
}

public record ContextMenuItem
{
	public string DisplayText { get; set; }
	public string Executable { get; set; }
	public string Arguments { get; set; } = "";
	public string Icon { get; set; } = "";
}

public record TaskbarConfiguration
{
	public const string ConfigKey = "Taskbar";
	public static readonly TaskbarConfiguration Empty = new();

	public string TaskManagerCommand { get; set; } = "";
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

	public static TaskbarConfiguration New(GlimpseAppSettings appSettings)
	{
		return new TaskbarConfiguration()
		{
			TaskManagerCommand = "xfce4-taskmanager",
			StartMenuLaunchIconName = "start-here",
			PinnedLaunchers = ImmutableList<string>.Empty,
			ContextMenu = ImmutableList.Create<ContextMenuItem>(
				new() { DisplayText = "Terminal", Executable = "x-terminal-emulator", Icon = "terminal" },
				new() { DisplayText = "Display", Executable = "xfce4-display-settings", Icon = "display" },
				new() { DisplayText = "Network Connections", Executable = "nm-connection-editor", Icon = "network-wireless" },
				new() { DisplayText = "Session & Startup", Executable = "xfce4-settings-manager", Arguments = "-d xfce-session-settings", Icon = "gnome-session" },
				new() { DisplayText = "separator" },
				new() { DisplayText = "Next Wallpaper", Executable = "gdbus", Arguments = $"call --session --dest org.{appSettings.ApplicationName} --object-path /org/{appSettings.ApplicationName} --method org.gtk.Actions.Activate \"NextWallpaper\" [] {{}}", Icon = "accessories-image-viewer" },
				new() { DisplayText = "Edit Glimpse config", Executable = "xdg-open", Arguments = appSettings.ConfigurationFilePath, Icon = "edit" },
				new() { DisplayText = "System Settings", Executable = "xfce4-settings-manager", Icon = "system-settings" },
				new() { DisplayText = "separator" },
				new() { DisplayText = "Shutdown or sign out", Executable = "xfce4-session-logout", Icon = "system-shutdown" })
		};
	}
}
