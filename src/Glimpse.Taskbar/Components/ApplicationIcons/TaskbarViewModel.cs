using System.Collections.Immutable;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Glimpse.Common.Xorg;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

internal record TaskbarViewModel
{
	public ImmutableList<SlotViewModel> Groups { get; init; } = ImmutableList<SlotViewModel>.Empty;
}

internal class WindowViewModel
{
	public string Title { get; init; }
	public ImageViewModel Icon { get; init; } = ImageViewModel.Empty;
	public IWindowRef WindowRef { get; init; }
	public AllowedWindowActions[] AllowedActions { get; init; }
	public ImageViewModel Screenshot { get; init; } = ImageViewModel.Empty;
	public bool DemandsAttention { get; init; }
}

internal record SlotContextMenuItemViewModel : IContextMenuItemViewModel<SlotContextMenuItemViewModel>
{
	public DesktopFileAction DesktopAction { get; set; }
	public string DesktopFilePath { get; set; }
	public string DisplayText { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public string Id { get; set; }
	public ImmutableList<SlotContextMenuItemViewModel> Children { get; set; } = ImmutableList<SlotContextMenuItemViewModel>.Empty;
}

internal record SlotViewModel
{
	public ImmutableList<WindowViewModel> Tasks { get; init; } = ImmutableList<WindowViewModel>.Empty;
	public SlotRef SlotRef { get; set; }
	public DesktopFile DesktopFile { get; init; }
	public bool DemandsAttention { get; init; }
	public ImageViewModel Icon { get; init; } = ImageViewModel.Empty;
	public ImmutableList<SlotContextMenuItemViewModel> ContextMenuItems { get; set; } = ImmutableList<SlotContextMenuItemViewModel>.Empty;

	public virtual bool Equals(SlotViewModel other) => ReferenceEquals(this, other);
}
