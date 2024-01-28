using System.Collections.Immutable;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Xorg;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

internal record TaskbarViewModel
{
	public ImmutableList<SlotViewModel> Groups { get; init; } = ImmutableList<SlotViewModel>.Empty;
}

internal class WindowViewModel
{
	public string Title { get; init; }
	public ImageViewModel Icon { get; init; }
	public IWindowRef WindowRef { get; init; }
	public AllowedWindowActions[] AllowedActions { get; init; }
	public ImageViewModel Screenshot { get; init; }
	public bool DemandsAttention { get; init; }
}

internal class TaskbarGroupContextMenuViewModel
{
	public bool IsPinned { get; init; }
	public Dictionary<string, ImageViewModel> ActionIcons { get; set; }
	public DesktopFile DesktopFile { get; init; }
	public ImageViewModel LaunchIcon { get; set; }
	public bool CanClose { get; set; }
}

internal record SlotViewModel
{
	public ImmutableList<WindowViewModel> Tasks { get; init; } = ImmutableList<WindowViewModel>.Empty;
	public SlotRef SlotRef { get; set; }
	public DesktopFile DesktopFile { get; init; }
	public bool DemandsAttention { get; init; }
	public ImageViewModel Icon { get; init; }
	public TaskbarGroupContextMenuViewModel ContextMenu { get; set; }

	public virtual bool Equals(SlotViewModel other) => ReferenceEquals(this, other);
}
