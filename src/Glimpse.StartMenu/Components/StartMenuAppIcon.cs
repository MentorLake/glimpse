using System.Reactive.Linq;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Glimpse.Common.Gtk.ForEach;
using Gtk;
using Pango;
using WrapMode = Pango.WrapMode;

namespace Glimpse.StartMenu.Components;

internal class StartMenuAppIcon : IGlimpseFlowBoxItem
{
	private readonly EventBox _root;

	public Widget Widget => _root;
	public IObservable<StartMenuAppContextMenuItem> ContextMenuItemActivated { get; }
	public IObservable<ImageViewModel> IconWhileDragging { get; }
	public string Id { get; }
	public StartMenuAppViewModel ViewModel { get; private set; }

	public StartMenuAppIcon(string id, IObservable<StartMenuAppViewModel> viewModelObservable)
	{
		Id = id;
		_root = new EventBox();
		_root.CanFocus = false;
		_root.AddClass("start-menu__app-icon-container");

		IconWhileDragging = viewModelObservable.Select(vm => vm.Icon).DistinctUntilChanged().Replay(1).AutoConnect();

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

		_root.Add(appIconContainer);

		viewModelObservable
			.TakeUntilDestroyed(_root)
			.Subscribe(f =>
			{
				ViewModel = f;
				name.Text = f.DesktopFile.Name;
			});

		_root.AppIcon(image, IconWhileDragging, 36);
		var contextMenu = ContextMenuFactory.Create(_root, viewModelObservable.Select(s => s.ContextMenuItems));
		ContextMenuItemActivated = contextMenu.ItemActivated;
	}
}
