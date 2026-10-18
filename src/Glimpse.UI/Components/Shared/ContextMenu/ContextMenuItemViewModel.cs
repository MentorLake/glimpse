using Glimpse.Libraries.Gtk;

namespace Glimpse.UI.Components.Shared.ContextMenu;

public record ContextMenuItemViewModel : IContextMenuItemViewModel<ContextMenuItemViewModel>
{
	public string DisplayText { get; set; }
	public ImageViewModel Icon { get; set; } = ImageViewModel.Empty;
	public string Id { get; set; }
	public bool IsChecked { get; set; }
	public bool IsEnabled { get; set; } = true;
	public ImmutableList<ContextMenuItemViewModel> Children { get; set; } = ImmutableList<ContextMenuItemViewModel>.Empty;
}

public interface IContextMenuItemViewModel<T> where T : IContextMenuItemViewModel<T>
{
	public string DisplayText { get; }
	public ImageViewModel Icon { get; }
	public string Id { get; }
	public bool IsChecked { get; }
	public bool IsEnabled { get; }
	public ImmutableList<T> Children { get; }
}
