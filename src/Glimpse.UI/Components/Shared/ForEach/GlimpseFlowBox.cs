using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using MentorLake.Gdk;
using MentorLake.Gtk;
using MentorLake.Gtk3;

namespace Glimpse.UI.Components.Shared.ForEach;

public class GlimpseFlowBox<TItem> where TItem : IGlimpseFlowBoxItem
{
	private readonly Subject<List<TItem>> _orderingChangedSubject = new();
	private readonly Subject<TItem> _dragBeginSubject = new();
	private readonly GtkFixedHandle _fixedContainer;
	private readonly GtkImageHandle _dragIconImage;
	private readonly List<GtkFlowBoxChildHandle> _flowboxChildWidgetCache = new();

	private bool _dragInitialized;
	private readonly Subject<TItem> _itemActivatedSubject = new();
	private bool _isLayoutRefreshQueued;
	private int _selectedChildIndex = -1;
	private List<GtkFlowBoxChildHandle> _visibleChildCache = new();

	public bool IsDragEnabled { get; set; }
	public int ItemsPerLine { get; set; } = 7;
	public int ItemSpacing { get; set; }
	public Func<TItem, bool> FilterFunc = _ => true;

	public IObservable<List<TItem>> OrderingChanged => _orderingChangedSubject;
	public IObservable<TItem> DragBeginObservable => _dragBeginSubject;
	public IObservable<TItem> ItemActivated => _itemActivatedSubject;
	public GtkFixedHandle Widget => _fixedContainer;

	public GlimpseFlowBox()
	{
		_dragIconImage = GtkImageHandle.New()
			.AddClass("taskbar__item");

		_fixedContainer = GtkFixedHandle.New()
			.SetCanFocus(false)
			.SetFocusChain(null)
			.Add(_dragIconImage);

		_fixedContainer.ObserveEvent(w => w.Signal_Focus()).Subscribe(e =>
		{
			if (_visibleChildCache.Count == 0)
			{
				e.ReturnValue = false;
			}
			else if (_fixedContainer.GetFocusChild() == null)
			{
				SelectChildByIndex(0);
				e.ReturnValue = true;
			}
			else if (e.Direction == GtkDirectionType.GTK_DIR_TAB_BACKWARD || e.Direction == GtkDirectionType.GTK_DIR_TAB_FORWARD)
			{
				SelectChildByIndex(-1);
				e.ReturnValue = false;
			}
			else
			{
				var numItems = _visibleChildCache.Count;
				var row = Math.Floor(_selectedChildIndex / (decimal) ItemsPerLine);
				var rowStartIndex = row * ItemsPerLine;
				var rowIndex = _selectedChildIndex - rowStartIndex;

				e.ReturnValue = true;

				if (e.Direction == GtkDirectionType.GTK_DIR_RIGHT)
				{
					if (rowIndex < ItemsPerLine - 1 && _selectedChildIndex < numItems - 1) SelectChildByIndex(_selectedChildIndex + 1);
				}
				else if (e.Direction == GtkDirectionType.GTK_DIR_LEFT)
				{
					if (rowIndex > 0) SelectChildByIndex(_selectedChildIndex - 1);
				}
				else if (e.Direction == GtkDirectionType.GTK_DIR_DOWN)
				{
					var newIndex = _selectedChildIndex + ItemsPerLine;
					if (newIndex >= numItems) return;
					SelectChildByIndex(newIndex);
				}
				else if (e.Direction == GtkDirectionType.GTK_DIR_UP)
				{
					var newIndex = _selectedChildIndex - ItemsPerLine;
					if (newIndex < 0) return;
					SelectChildByIndex(newIndex);
				}
			}
		});

		_fixedContainer.ObserveEvent(w => w.Signal_KeyPressEvent())
			.Select(e => e.Event.Dereference())
			.Where(e => e.keyval == GdkConstants.KEY_KP_Enter || e.keyval == GdkConstants.KEY_Return)
			.Subscribe(_ =>
			{
				var index = _selectedChildIndex == -1 ? 0 : _selectedChildIndex;
				_itemActivatedSubject.OnNext(_visibleChildCache[index].GetManagedData<TItem>("Item"));
			});
	}

	public void AddOrUpdate(TItem item, int newIndex)
	{
		var target = _flowboxChildWidgetCache.FirstOrDefault(c => c.GetChild() == item.Widget);

		if (target == null)
		{
			AddItem(item, newIndex);
		}
		else
		{
			target.SetIndex(newIndex);
		}

		QueueLayoutRefresh();
	}

	public void RemoveItem(TItem item)
	{
		var flowBoxChild = _flowboxChildWidgetCache.First(c => c.GetChild() == item.Widget);
		flowBoxChild.Destroy();
		_flowboxChildWidgetCache.Remove(flowBoxChild);
		QueueLayoutRefresh();
	}

	private void AddItem(TItem item, int index)
	{
		var flowBoxChild = GtkFlowBoxChildHandle.New().AddMany(item.Widget);
		flowBoxChild.SetIndex(index);
		flowBoxChild.SetManagedData("Item", item);
		flowBoxChild.ShowAll();
		flowBoxChild.Signal_Destroy().Take(1).Subscribe(_ => QueueLayoutRefresh());
		flowBoxChild
			.AddClass("taskbar__item")
			.SetCanFocus(true)
			.SetSensitive(true);

		_flowboxChildWidgetCache.Add(flowBoxChild);
		_fixedContainer.Put(flowBoxChild, 0, 0);

		var dragGesture = GtkGestureDragHandle.New(item.Widget);
		var pressGesture = GtkGestureMultiPressHandle.New(item.Widget);
		pressGesture.SetButton(1);
		pressGesture.Signal_Released().TakeUntilDestroyed(flowBoxChild).Subscribe(r => _itemActivatedSubject.OnNext(item));

		dragGesture.Signal_DragUpdate()
			.TakeUntilDestroyed(flowBoxChild)
			.Where(_ => IsDragEnabled)
			.Subscribe(e =>
			{
				if (!_dragInitialized)
				{
					var xThresholdMet = e.OffsetX > 5 || e.OffsetX < -5;
					var yThresholdMet = e.OffsetY > 5 || e.OffsetY < -5;
					if (!xThresholdMet && !yThresholdMet) return;
				}

				OnDragMotion(dragGesture, flowBoxChild, item, (int)e.OffsetX, (int)e.OffsetY);
				dragGesture.SetState(GtkEventSequenceState.GTK_EVENT_SEQUENCE_CLAIMED);
				pressGesture.SetState(GtkEventSequenceState.GTK_EVENT_SEQUENCE_DENIED);
			});

		dragGesture.Signal_DragBegin()
			.TakeUntilDestroyed(flowBoxChild)
			.Where(_ => IsDragEnabled)
			.Subscribe(t =>
			{
				_dragInitialized = false;
			});

		dragGesture.Signal_End()
			.TakeUntilDestroyed(flowBoxChild)
			.Where(static e => e.Self.GetSequenceState(e.Sequence) == GtkEventSequenceState.GTK_EVENT_SEQUENCE_CLAIMED)
			.Subscribe(_ => OnDragEndInternal(flowBoxChild));
	}

	private void OnDragEndInternal(GtkFlowBoxChildHandle flowBoxChild)
	{
		_dragIconImage.Hide();
		flowBoxChild.GetChild().SetOpacity(1);

		_orderingChangedSubject.OnNext(
			_flowboxChildWidgetCache
				.Where(static c => c.GetMapped())
				.OrderBy(static c => GtkExtensions.GetIndex(c))
				.Select(static c => c.GetManagedData<TItem>("Item"))
				.ToList());
	}

	private void OnDragMotion(GtkGestureDragHandle dragGesture, GtkFlowBoxChildHandle flowBoxChild, TItem draggable, int iconX, int iconY)
	{
		var lastPosition = _dragIconImage.GetAllocationRect();
		var lastX = lastPosition.X + _dragIconImage.GetMarginStart();
		var lastY = lastPosition.Y + _dragIconImage.GetMarginTop();
		lastPosition.X = lastX;
		lastPosition.Y = lastY;

		if (!_dragInitialized)
		{
			_fixedContainer.GetToplevel().ToHandle<GtkWindowHandle>().SetFocus(null);
			_dragIconImage.SetFromPixbuf(draggable.DragIcon);
			_dragIconImage.ShowAll();
			_dragBeginSubject.OnNext(draggable);
			_dragInitialized = true;
			flowBoxChild.GetChild().SetOpacity(0);
			dragGesture.GetStartPoint(out var startX, out var startY);
			lastPosition.Location = new Point((int) startX, (int) startY);
			QueueLayoutRefresh();
		}

		flowBoxChild.TranslateCoordinates(_fixedContainer, iconX, iconY, out var x, out var y);
		var imageRect = new Rectangle(_fixedContainer.GetAllocationRect().Constrain(new Rectangle(new Point(x, y), lastPosition.Size)), lastPosition.Size);
		var children = _flowboxChildWidgetCache.Where(static c => c.GetMapped()).OrderBy(static c => GtkExtensions.GetIndex(c)).ToList();
		var movedLeft = imageRect.X < lastPosition.X;
		var movedRight = imageRect.X > lastPosition.X;
		var movedUp = imageRect.Y < lastPosition.Y;
		var movedDown = imageRect.Y > lastPosition.Y;
		var shrunkenImageRect = imageRect.TrimSides((int)(imageRect.Width * .5));

		Point? corner = null;

		if (movedUp && movedRight) corner = shrunkenImageRect.UpperRight();
		else if (movedUp && movedLeft) corner = shrunkenImageRect.UpperLeft();
		else if (movedDown && movedLeft) corner = shrunkenImageRect.LowerLeft();
		else if (movedDown && movedRight) corner = shrunkenImageRect.LowerRight();
		else if (movedLeft) corner = shrunkenImageRect.UpperLeft();
		else if (movedRight) corner = shrunkenImageRect.UpperRight();
		else if (movedUp) corner = shrunkenImageRect.UpperLeft();
		else if (movedDown) corner = shrunkenImageRect.LowerLeft();

		if (corner.HasValue && children.FindIndex(c => ToLocalCoords(c).Contains(corner.Value)) is var hoveredIndex && hoveredIndex != -1 && hoveredIndex != GtkExtensions.GetIndex(flowBoxChild))
		{
			children.Remove(flowBoxChild);
			children.Insert(hoveredIndex, flowBoxChild);
			for (var i=0; i<children.Count; i++) children[i].SetIndex(i);
			QueueLayoutRefresh();
		}

		_fixedContainer.Move(_dragIconImage, imageRect.X, imageRect.Y);
	}

	private Rectangle ToLocalCoords(GtkWidgetHandle widget)
	{
		widget.GetAllocation(out var widgetAllocation);
		_fixedContainer.GetAllocation(out var containerAllocation);
		return new Rectangle(widgetAllocation.Value.x - containerAllocation.Value.x, widgetAllocation.Value.y - containerAllocation.Value.y, widgetAllocation.Value.width, widgetAllocation.Value.height);
	}

	public void InvalidateFilter()
	{
		QueueLayoutRefresh();
	}

	public void SelectChildByIndex(int index)
	{
		_selectedChildIndex = index;

		if (index >= 0)
		{
			_visibleChildCache[index].GrabFocus();
		}
	}

	private void QueueLayoutRefresh()
	{
		if (_isLayoutRefreshQueued) return;
		_isLayoutRefreshQueued = true;

		GLibExt.Defer(() =>
		{
			_isLayoutRefreshQueued = false;
			RefreshLayout();
		}, 0);
	}

	private void RefreshLayout()
	{
		var visibleChildren = new List<GtkFlowBoxChildHandle>(_flowboxChildWidgetCache.Count);

		foreach (var c in _flowboxChildWidgetCache)
		{
			var shouldShow = FilterFunc(c.GetManagedData<TItem>("Item"));
			if (GtkExtensions.GetIndex(c) == -1) c.Hide();
			else if (!c.GetMapped() && shouldShow)
			{
				visibleChildren.Add(c);
				c.ShowAll();
			}
			else if (c.GetMapped() && !shouldShow) c.Hide();
			else if (c.GetMapped()) visibleChildren.Add(c);
		}

		var sortedChildren = visibleChildren.OrderBy(static i => GtkExtensions.GetIndex(i)).ToList();
		_visibleChildCache = sortedChildren;
		if (!sortedChildren.Any()) return;
		var currentY = 0;
		var allChunks = sortedChildren.Chunk(ItemsPerLine).ToList();
		var firstRow = allChunks.First();
		var firstItem = firstRow.First();
		firstItem.GetPreferredWidth(out var itemWidth, out _);
		firstItem.GetPreferredHeight(out var itemHeight, out _);

		foreach (var row in allChunks)
		{
			var currentX = 0;

			foreach (var currentItem in row)
			{
				_fixedContainer.Move(currentItem, currentX, currentY);
				currentX += itemWidth + ItemSpacing;
			}

			currentY += itemHeight + ItemSpacing;
		}

		var newWidth = firstRow.Length * itemWidth + ItemSpacing * (firstRow.Length - 1);
		var newHeight = allChunks.Count * itemHeight + ItemSpacing * (allChunks.Count - 1);
		_fixedContainer.SetSizeRequest(newWidth, newHeight);
	}
}
