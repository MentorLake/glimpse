using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.UI.Components.Shared.ContextMenu;
using Glimpse.UI.Components.Shared.ForEach;
using MentorLake.GdkPixbuf;
using MentorLake.Gtk;
using MentorLake.Pango;

namespace Glimpse.UI.Components.Shared;

public class AppIconViewModel<TContextMenuViewModel>
{
	public string Text { get; set; }
	public IconInfo IconInfo { get; set; }
	public ImmutableList<TContextMenuViewModel> ContextMenuItems { get; set; }
}

public class AppIcon<TContextMenuViewModel> : IGlimpseFlowBoxItem where TContextMenuViewModel : IContextMenuItemViewModel<TContextMenuViewModel>
{
	private Icon _activatedIcon;
	private Icon _primaryIcon;

	private readonly int _iconSize;
	private readonly GtkEventBoxHandle _root;
	private readonly GtkLabelHandle _name;
	private readonly ContextMenu<TContextMenuViewModel> _contextMenu;
	private readonly GtkImageHandle _imageWidget;

	public string Id { get; }
	public GtkWidgetHandle Widget => _root;
	public GdkPixbufHandle DragIcon { get; set; }
	public IObservable<TContextMenuViewModel> ContextMenuItemActivated { get; }

	public AppIcon(string id, int iconSize, IObservable<AppIconViewModel<TContextMenuViewModel>> viewModelObs)
	{
		Id = id;
		_iconSize = iconSize;

		_root = GtkEventBoxHandle.New()
			.SetCanFocus(false);

		_name = GtkLabelHandle.New("")
			.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END)
			.SetLines(2)
			.SetLineWrap(true)
			.SetLineWrapMode(PangoWrapMode.PANGO_WRAP_WORD)
			.SetMaxWidthChars(1)
			.SetJustify(GtkJustification.GTK_JUSTIFY_CENTER);

		_imageWidget = GtkImageHandle.New();
		_contextMenu = ContextMenuFactory.Create<TContextMenuViewModel>(_root);
		ContextMenuItemActivated = _contextMenu.ItemActivated;

		_root.Add(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
			.AddMany(_imageWidget, _name)
			.SetValign(GtkAlign.GTK_ALIGN_CENTER)
			.SetHalign(GtkAlign.GTK_ALIGN_CENTER));

		_name.SetNoShowAll(true);
		_name.Hide();

		_root.ShowAll();
		_root.AddButtonStates();
		_root.ObserveEvent(w => w.Signal_EnterNotifyEvent()).SubscribeDebug(_ => _root.QueueDraw());
		_root.ObserveEvent(w => w.Signal_LeaveNotifyEvent()).SubscribeDebug(_ => _root.QueueDraw());
		_root.ObserveEvent(w => w.Signal_ButtonPressEvent()).SubscribeDebug(_ => _root.QueueDraw());
		_root.ObserveEvent(w => w.Signal_ButtonPressEvent()).SubscribeDebug(_ => _imageWidget.SetFromPixbuf(_activatedIcon.Image));
		_root.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).SubscribeDebug(_ => _imageWidget.SetFromPixbuf(_primaryIcon.Image));
		_root.ObserveEvent(w => w.Signal_LeaveNotifyEvent()).SubscribeDebug(_ => _imageWidget.SetFromPixbuf(_primaryIcon.Image));
		_root.ObserveEvent(w => w.Signal_SizeAllocate())
			.DistinctUntilChanged((x, y) => x.Allocation.DereferenceValue().height == y.Allocation.DereferenceValue().height)
			.Subscribe(e => SetDragIcon(e.Allocation.DereferenceValue().height));

		viewModelObs.Select(vm => vm.Text).TakeUntilDestroyed(_root).SubscribeDebug(SetText);
		viewModelObs.Select(vm => vm.IconInfo).TakeUntilDestroyed(_root).SubscribeDebug(SetIcon);
		viewModelObs.Select(vm => vm.ContextMenuItems).TakeUntilDestroyed(_root).SubscribeDebug(SetContextMenu);
	}

	private void SetDragIcon(int size)
	{
		DragIcon = IconManager.GetDefault().GetIcon(_primaryIcon.Info, size).Image;
	}

	private void SetIcon(IconInfo iconInfo)
	{
		var manager = IconManager.GetDefault();

		if (manager.TryGetUpdatedIcon(iconInfo, _iconSize, _primaryIcon, out var newIcon))
		{
			_primaryIcon = newIcon;
			_imageWidget.SetFromPixbuf(newIcon.Image);
			if (_root.GetMapped()) SetDragIcon(_root.GetAllocatedHeight());
		}

		if (manager.TryGetUpdatedIcon(iconInfo, _iconSize - 6, _activatedIcon, out var newActivatedIcon))
		{
			_activatedIcon = newActivatedIcon;
		}
	}

	private void SetText(string text)
	{
		_name.SetText(text);

		if (string.IsNullOrEmpty(text)) _name.Hide();
		else _name.Show();
	}

	private void SetContextMenu(ImmutableList<TContextMenuViewModel> contextMenuItems)
	{
		_contextMenu.UpdateItems(contextMenuItems);
	}
}
