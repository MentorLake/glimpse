using System.Collections.Immutable;
using Glimpse.Freedesktop.DesktopEntries;
using Glimpse.UI.State;
using Glimpse.Xorg;

namespace Glimpse.Taskbar.Components;

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
