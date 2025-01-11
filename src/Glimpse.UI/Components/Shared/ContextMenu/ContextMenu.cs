using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Collections.Generic;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.Shared.ContextMenu;

public class ContextMenu<T> where T : IContextMenuItemViewModel<T>
{
	private readonly GtkMenuHandle _root = GtkMenuHandle.New();
	private readonly Subject<T> _itemActivatedSubject = new();
	private bool _isStale;
	private ImmutableList<T> _viewModel;

	public IObservable<T> ItemActivated => _itemActivatedSubject;
	public GtkMenuHandle Widget => _root;

	private bool IsSeparator(T i) => i.DisplayText == "separator" || i.Id == "separator";
	private bool IsHeader(T i) => i.Id == "header";

	public bool HasItems => _viewModel?.Any() ?? false;

	public void PopupContextMenu(GtkWidgetHandle parent)
	{
		if (_isStale)
		{
			_isStale = false;
			Rebuild();
		}

		Widget.PopupAtPointer(null);

		Widget.Signal_Unmap()
			.Take(1)
			.TakeUntilDestroyed(parent)
			.Subscribe(_ => parent.SetStateFlags(GtkStateFlags.GTK_STATE_FLAG_NORMAL, true));
	}

	public void UpdateItems(ImmutableList<T> items)
	{
		if (_viewModel == items) return;
		_viewModel = items;

		if (Widget.GetMapped())
		{
			Rebuild();
			_isStale = false;
		}
		else
		{
			_isStale = true;
		}
	}

	private void Rebuild()
	{
		_root.RemoveAllChildren();
		var menuItems = CreateMenuItems(_viewModel);
		_root.AddMany(menuItems);
		_root.ShowAll();
	}

	private GtkWidgetHandle[] CreateMenuItems(ImmutableList<T> vm)
	{
		_viewModel = vm;

		var cleanedUpMenuItems = vm
			.SkipWhile(IsSeparator)
			.Reverse()
			.SkipWhile(i => IsSeparator(i) || IsHeader(i))
			.Reverse()
			.SkipConsecutiveDuplicates((x, y) => IsSeparator(x) && IsSeparator(y))
			.ToList();

		var items = new List<GtkMenuItemHandle>(vm.Count);
		var shouldReserveSpaceForImages = cleanedUpMenuItems.Any(i => !i.Icon.IsNull());

		foreach (var item in cleanedUpMenuItems)
		{
			if (IsSeparator(item)) items.Add(GtkSeparatorMenuItemHandle.New());
			else if (IsHeader(item)) items.Add(CreateHeader(item));
			else items.Add(CreateMenuItem(item, shouldReserveSpaceForImages));
		}

		return items.Cast<GtkWidgetHandle>().ToArray();
	}

	private GtkMenuItemHandle CreateHeader(IContextMenuItemViewModel<T> item)
	{
		var headerLabel = GtkLabelHandle.New(item.DisplayText)
			.SetHalign(GtkAlign.GTK_ALIGN_START)
			.AddClass("header-menu-item-label");

		var header = GtkSeparatorMenuItemHandle.New()
			.AddClass("header-menu-item")
			.Add(headerLabel);

		return header;
	}

	private GtkMenuItemHandle CreateMenuItem(T item, bool shouldReserveSpaceForImages)
	{
		var box = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 6);

		if (shouldReserveSpaceForImages)
		{
			var image = GtkImageHandle.New();
			image.BindViewModel(Observable.Return(item.Icon), 18, false);
			box.Add(image);
		}

		box.Add(GtkLabelHandle.New(item.DisplayText));

		var menuItem = GtkMenuItemHandle.New()
			.Add(box)
			.ObserveEvent(x => x.Signal_Activate(), _ => _itemActivatedSubject.OnNext(item));

		if (item.Children != null && item.Children.Any())
		{
			menuItem.SetSubmenu(GtkMenuHandle.New().AddMany(CreateMenuItems(item.Children)));
		}

		return menuItem;
	}
}
