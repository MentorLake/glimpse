using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Common.System.Collections.Generic;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Gtk.ContextMenu;

public class ContextMenu<T> : Menu where T : IContextMenuItemViewModel<T>
{
	private readonly Subject<T> _itemActivatedSubject = new();
	public IObservable<T> ItemActivated => _itemActivatedSubject;

	private bool IsSeparator(T i) => i.DisplayText == "separator" || i.Id == "separator";
	private bool IsHeader(T i) => i.Id == "header";

	public void BindViewModel(IObservable<ImmutableList<T>> viewModelObs)
	{
		viewModelObs.Subscribe(vm =>
		{
			this.RemoveAllChildren();
			this.AddMany(CreateMenuItems(vm));
			ShowAll();
		});
	}

	private Widget[] CreateMenuItems(ImmutableList<T> vm)
	{
		var cleanedUpMenuItems = vm
			.SkipWhile(IsSeparator)
			.Reverse()
			.SkipWhile(i => IsSeparator(i) || IsHeader(i))
			.Reverse()
			.SkipConsecutiveDuplicates((x, y) => IsSeparator(x) && IsSeparator(y))
			.ToList();

		var items = new List<MenuItem>(vm.Count);
		var shouldReserveSpaceForImages = cleanedUpMenuItems.Any(i => !i.Icon.IsNull());

		foreach (var item in cleanedUpMenuItems)
		{
			if (IsSeparator(item)) items.Add(new SeparatorMenuItem());
			else if (IsHeader(item)) items.Add(CreateHeader(item));
			else items.Add(CreateMenuItem(item, shouldReserveSpaceForImages));
		}

		return items.Cast<Widget>().ToArray();
	}

	private MenuItem CreateHeader(IContextMenuItemViewModel<T> item)
	{
		var headerLabel = new Label(item.DisplayText);
		headerLabel.Halign = Align.Start;
		headerLabel.StyleContext.AddClass("header-menu-item-label");

		var header = new SeparatorMenuItem();
		header.StyleContext.AddClass("header-menu-item");
		header.Add(headerLabel);
		return header;
	}

	private MenuItem CreateMenuItem(T item, bool shouldReserveSpaceForImages)
	{
		var box = new Box(Orientation.Horizontal, 6);

		if (shouldReserveSpaceForImages)
		{
			var image = new Image();
			image.BindViewModel(Observable.Return(item.Icon), 18, false);
			box.Add(image);
		}

		box.Add(new Label(item.DisplayText));

		var menuItem = new MenuItem();
		menuItem.Add(box);
		menuItem.Name = item.Id;
		menuItem.Events().Activated.TakeUntilDestroyed(menuItem).Subscribe(_ => _itemActivatedSubject.OnNext(item));

		if (item.Children != null && item.Children.Any())
		{
			menuItem.Submenu = new Menu().AddMany(CreateMenuItems(item.Children));
		}

		return menuItem;
	}
}
