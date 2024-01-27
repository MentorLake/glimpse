using System.Collections.Immutable;
using Glimpse.Common.Freedesktop.DesktopEntries;
using Glimpse.Common.Freedesktop.Xorg;
using Glimpse.Common.Gtk;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

public record TaskbarViewModel
{
	public ImmutableList<SlotViewModel> Groups { get; init; } = ImmutableList<SlotViewModel>.Empty;
}

public class WindowViewModel
{
	public string Title { get; init; }
	public ImageViewModel Icon { get; init; }
	public IWindowRef WindowRef { get; init; }
	public AllowedWindowActions[] AllowedActions { get; init; }
	public ImageViewModel Screenshot { get; init; }
	public bool DemandsAttention { get; init; }
}

public class TaskbarGroupContextMenuViewModel
{
	public bool IsPinned { get; init; }
	public Dictionary<string, ImageViewModel> ActionIcons { get; set; }
	public DesktopFile DesktopFile { get; init; }
	public ImageViewModel LaunchIcon { get; set; }
	public bool CanClose { get; set; }
}

public record SlotViewModel
{
	public ImmutableList<WindowViewModel> Tasks { get; init; } = ImmutableList<WindowViewModel>.Empty;
	public SlotRef SlotRef { get; set; }
	public DesktopFile DesktopFile { get; init; }
	public bool DemandsAttention { get; init; }
	public ImageViewModel Icon { get; init; }
	public TaskbarGroupContextMenuViewModel ContextMenu { get; set; }

	public virtual bool Equals(SlotViewModel other) => ReferenceEquals(this, other);
}
