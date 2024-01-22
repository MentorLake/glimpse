using System.Reactive.Linq;
using Glimpse.UI;
using Glimpse.UI.Components.Shared.ForEach;
using Glimpse.UI.State;
using Gtk;
using Pango;
using WrapMode = Pango.WrapMode;

namespace Glimpse.StartMenu.Components;

public class StartMenuAppIcon : EventBox, IForEachDraggable
{
	public StartMenuAppIcon(IObservable<StartMenuAppViewModel> viewModelObservable)
	{
		CanFocus = false;

		this.AddClass("start-menu__app-icon-container");

		var name = new Label();
		name.Ellipsize = EllipsizeMode.End;
		name.Lines = 2;
		name.LineWrap = true;
		name.LineWrapMode = WrapMode.Word;
		name.MaxWidthChars = 1;
		name.Justify = Justification.Center;

		var iconObservable = viewModelObservable.Select(vm => vm.Icon).DistinctUntilChanged().Replay(1);
		var image = new Image();
		image.SetSizeRequest(36, 36);

		var appIconContainer = new Box(Orientation.Vertical, 0);
		appIconContainer.AddMany(image, name);
		appIconContainer.Valign = Align.Center;
		appIconContainer.Halign = Align.Center;

		Add(appIconContainer);

		ContextMenuRequested = this.CreateContextMenuObservable()
			.TakeUntilDestroyed(this)
			.WithLatestFrom(viewModelObservable)
			.Select(t => t.Second);

		viewModelObservable
			.TakeUntilDestroyed(this)
			.Subscribe(f => name.Text = f.DesktopFile.Name);

		this.AppIcon(image, iconObservable, 36);
		iconObservable.Connect();
		IconWhileDragging = iconObservable;
	}

	public IObservable<StartMenuAppViewModel> ContextMenuRequested { get; }
	public IObservable<ImageViewModel> IconWhileDragging { get; }
}
