using System.Reactive.Linq;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Glimpse.Common.Gtk.ForEach;
using Gtk;
using Pango;
using WrapMode = Pango.WrapMode;

namespace Glimpse.StartMenu.Components;

internal class StartMenuAppIcon : EventBox, IForEachDraggable
{
	public StartMenuAppIcon(IObservable<StartMenuAppViewModel> viewModelObservable)
	{
		CanFocus = false;
		IconWhileDragging = viewModelObservable.Select(vm => vm.Icon).DistinctUntilChanged().Replay(1).AutoConnect();

		this.AddClass("start-menu__app-icon-container");

		var name = new Label();
		name.Ellipsize = EllipsizeMode.End;
		name.Lines = 2;
		name.LineWrap = true;
		name.LineWrapMode = WrapMode.Word;
		name.MaxWidthChars = 1;
		name.Justify = Justification.Center;

		var image = new Image();
		image.SetSizeRequest(36, 36);

		var appIconContainer = new Box(Orientation.Vertical, 0);
		appIconContainer.AddMany(image, name);
		appIconContainer.Valign = Align.Center;
		appIconContainer.Halign = Align.Center;

		Add(appIconContainer);

		viewModelObservable
			.TakeUntilDestroyed(this)
			.Subscribe(f => name.Text = f.DesktopFile.Name);

		this.AppIcon(image, IconWhileDragging, 36);
		var contextMenu = ContextMenuFactory.Create(this, viewModelObservable.Select(s => s.ContextMenuItems));
		ContextMenuItemActivated = contextMenu.ItemActivated;
	}

	public IObservable<StartMenuAppContextMenuItem> ContextMenuItemActivated { get; }
	public IObservable<ImageViewModel> IconWhileDragging { get; }
}
