using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Gtk.ForEach;

public class ForEachFlowBoxCustom<TWidget> where TWidget : Widget, IForEachDraggable
{
	private readonly Subject<List<FlowBoxChild>> _orderingChangedSubject = new();
	private readonly Subject<TWidget> _dragBeginSubject = new();
	private readonly Fixed _fixedContainer;
	private readonly Image _dragIconImage;
	private ImageViewModel _dragImageViewModel;
	private bool _dragInitialized;
	private readonly Subject<FlowBoxChild> _itemActivatedSubject = new();

	public bool IsDragEnabled { get; set; }
	public int ItemsPerLine { get; set; } = 7;
	public int ItemSpacing { get; set; } = 0;
	public Func<FlowBoxChild, bool> FilterFunc = _ => true;

	public IObservable<List<FlowBoxChild>> OrderingChanged => _orderingChangedSubject;
	public IObservable<TWidget> DragBeginObservable => _dragBeginSubject;
	public IObservable<FlowBoxChild> ItemActivated => _itemActivatedSubject;
	public Fixed Widget => _fixedContainer;
	private IEnumerable<FlowBoxChild> AllChildItems => _fixedContainer.Children.Where(c => c != _dragIconImage).Cast<FlowBoxChild>();

	public ForEachFlowBoxCustom()
	{
		_dragIconImage = new Image().AddClass("taskbar__item");
		_fixedContainer = new Fixed();
		_fixedContainer.Add(_dragIconImage);
		_fixedContainer.ObserveEvent(x => x.Events().Mapped).Subscribe(_ => InvalidateFilter());
	}

	public void AddOrUpdate(TWidget widget, int newIndex)
	{
		var target = AllChildItems.FirstOrDefault(c => c.Child == widget);

		if (target == null)
		{
			AddItem(widget, newIndex);
		}
		else
		{
			target.SetIndex(newIndex);
		}

		RefreshLayout();
	}

	public void RemoveItem(TWidget childWidget)
	{
		var flowBoxChild = AllChildItems.First(c => c.Child == childWidget);
		_fixedContainer.Remove(flowBoxChild);
		flowBoxChild.Destroy();
	}

	private void AddItem(TWidget childWidget, int index)
	{
		var flowBoxChild = new FlowBoxChild().AddMany(childWidget).AddClass("taskbar__item");
		flowBoxChild.SetIndex(index);
		flowBoxChild.ShowAll();

		flowBoxChild.ObserveEvent(x => x.Events().SizeAllocated)
			.DistinctUntilChanged(a => a.Allocation.Width + "x" + a.Allocation.Height)
			.Subscribe(_ => RefreshLayout());

		flowBoxChild.Events().Destroyed.Take(1).Subscribe(_ => RefreshLayout());

		_fixedContainer.Add(flowBoxChild);

		var dragGesture = new GestureDrag(childWidget);
		var pressGesture = new GestureMultiPress(childWidget) { Button = 1 };

		pressGesture.Events().Released
			.Subscribe(r => _itemActivatedSubject.OnNext(flowBoxChild));

		dragGesture.Events().DragUpdate
			.Where(_ => IsDragEnabled)
			.Subscribe(e =>
			{
				OnDragMotion(dragGesture, flowBoxChild, (int)e.OffsetX, (int)e.OffsetY);
				dragGesture.SetState(EventSequenceState.Claimed);
				pressGesture.SetState(EventSequenceState.Denied);
			});

		dragGesture.Events().DragBegin
			.Where(_ => IsDragEnabled)
			.WithLatestFrom(childWidget.IconWhileDragging)
			.Subscribe(t =>
			{
				_dragImageViewModel = t.Second;
				_dragInitialized = false;
			});

		dragGesture.Events().End
			.Where(e => dragGesture.GetSequenceState(e.Sequence) == EventSequenceState.Claimed)
			.Subscribe(_ => OnDragEndInternal(flowBoxChild));
	}

	private void OnDragEndInternal(FlowBoxChild flowBoxChild)
	{
		_dragIconImage.Hide();
		flowBoxChild.Child.Opacity = 1;

		_orderingChangedSubject.OnNext(
			_fixedContainer.Children
				.Where(c => c.IsMapped)
				.Where(c => c != _dragIconImage)
				.Cast<FlowBoxChild>()
				.OrderBy(c => c.GetIndex())
				.ToList());
	}

	private void OnDragMotion(GestureDrag dragGesture, FlowBoxChild flowBoxChild, int iconX, int iconY)
	{
		var lastPosition = _dragIconImage.Allocation;
		var lastX = lastPosition.X + _dragIconImage.MarginStart;
		var lastY = lastPosition.Y + _dragIconImage.MarginTop;
		lastPosition.X = lastX;
		lastPosition.Y = lastY;

		if (!_dragInitialized)
		{
			var size = flowBoxChild.Allocation.Height;
			_dragIconImage.ApplyViewModel(_dragImageViewModel, size, size);
			_dragIconImage.ShowAll();
			_dragBeginSubject.OnNext(flowBoxChild.Child as TWidget);
			_dragInitialized = true;
			flowBoxChild.Child.Opacity = 0;
			dragGesture.GetStartPoint(out var startX, out var startY);
			lastPosition.Location = new Point((int) startX, (int) startY);
			RefreshLayout();
		}

		flowBoxChild.TranslateCoordinates(_fixedContainer, iconX, iconY, out var x, out var y);
		var imageRect = new Rectangle(_fixedContainer.Allocation.Constrain(new Rectangle(new Point(x, y), lastPosition.Size)), lastPosition.Size);
		var children = AllChildItems.Where(c => c.IsMapped).OrderBy(c => c.GetIndex()).ToList();
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

		if (corner.HasValue && children.FindIndex(c => ToLocalCoords(c).Contains(corner.Value)) is var hoveredIndex && hoveredIndex != -1 && hoveredIndex != flowBoxChild.GetIndex())
		{
			children.Remove(flowBoxChild);
			children.Insert(hoveredIndex, flowBoxChild);
			for (var i=0; i<children.Count; i++) children[i].SetIndex(i);
			RefreshLayout();
		}

		_fixedContainer.Move(_dragIconImage, imageRect.X, imageRect.Y);
	}

	private Rectangle ToLocalCoords(Widget widget)
	{
		var a = widget.Allocation;
		return new Rectangle(a.X - _fixedContainer.Allocation.X, a.Y - _fixedContainer.Allocation.Y, a.Width, a.Height);
	}

	public void InvalidateFilter()
	{
		foreach (var c in AllChildItems)
		{
			var shouldShow = FilterFunc(c);
			if (!c.IsMapped && shouldShow) c.ShowAll();
			else if (c.IsMapped && !shouldShow) c.Hide();
		}

		RefreshLayout();
	}

	private void RefreshLayout()
	{
		var sortedChildren = AllChildItems.Where(c => c.IsMapped).OrderBy(i => i.GetIndex()).ToList();
		if (!sortedChildren.Any()) return;
		var currentY = 0;
		var allChunks = sortedChildren.Chunk(ItemsPerLine).ToList();
		var firstRow = allChunks.First();
		var firstItem = firstRow.First();
		var itemWidth = firstItem.Allocation.Width;
		var itemHeight = firstItem.Allocation.Height;

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

		var newWidth = firstRow.Length * firstItem.Allocation.Width + ItemSpacing * (firstRow.Length - 1);
		var newHeight = allChunks.Count * firstItem.Allocation.Height + ItemSpacing * (allChunks.Count - 1);
		_fixedContainer.SetSizeRequest(newWidth, newHeight);
	}
}
