using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
using Glimpse.Common.System.Reactive;
using Gtk;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Gtk.ForEach;

public class ForEachFlowBox<TViewModel, TWidget, TKey> where TWidget : Widget, IForEachDraggable where TKey : IEquatable<TKey>
{
	private readonly Subject<List<TViewModel>> _orderingChangedSubject = new();
	private readonly Subject<TWidget> _dragBeginSubject = new();
	private readonly ObservableProperty<bool> _disableDragAndDrop = new(false);
	private readonly FlowBox _flowBox;
	private readonly Image _dragIconImage;
	private readonly Fixed _rootContainer;
	private ImageViewModel _dragImageViewModel;
	private bool _dragInitialized;

	public Fixed Root => _rootContainer;
	public FlowBox Widget => _flowBox;

	public IObservable<List<TViewModel>> OrderingChanged => _orderingChangedSubject;
	public IObservable<TWidget> DragBeginObservable => _dragBeginSubject;

	public IObservable<bool> DisableDragAndDrop
	{
		get => _disableDragAndDrop;
		set => _disableDragAndDrop.UpdateSource(value);
	}

	public ForEachFlowBox(IObservable<IList<TViewModel>> itemsObservable, Func<TViewModel, TKey> trackBy, Func<IObservable<TViewModel>, TWidget> widgetFactory)
	{
		_dragIconImage = new Image().AddClass("taskbar__item");

		_flowBox = new FlowBox();
		_flowBox.SortFunc = SortByItemIndex;

		_rootContainer = new Fixed();
		_rootContainer.Put(_flowBox, 0, 0);
		_rootContainer.Put(_dragIconImage, 0, 0);

		itemsObservable.UnbundleMany(trackBy).Subscribe(itemObservable =>
		{
			var childWidget = widgetFactory(itemObservable.Select(i => i.Item1).DistinctUntilChanged());
			var flowBoxChild = new FlowBoxChild().AddMany(childWidget).AddClass("taskbar__item");
			flowBoxChild.Data[ForEachDataKeys.Index] = 0;
			flowBoxChild.ShowAll();
			_flowBox.Add(flowBoxChild);

			var childDragGesture = new GestureDrag(childWidget);

			childDragGesture.Events().DragUpdate
				.Subscribe(e => OnDragMotion(childDragGesture, flowBoxChild, (int) e.OffsetX, (int) e.OffsetY));

			childDragGesture.Events().DragBegin
				.WithLatestFrom(childWidget.IconWhileDragging)
				.Subscribe(t =>
				{
					_dragImageViewModel = t.Second;
					_dragInitialized = false;
				});

			childDragGesture.Events().DragEnd
				.Subscribe(e => OnDragEndInternal(flowBoxChild));

			_disableDragAndDrop
				.TakeUntil(itemObservable.TakeLast(1))
				.Subscribe(b => ToggleDragSource(flowBoxChild, b));

			itemObservable
				.Select(i => i.Item1)
				.DistinctUntilChanged()
				.Subscribe(i => flowBoxChild.Data[ForEachDataKeys.Model] = i);

			itemObservable
				.Select(i => i.Item2)
				.DistinctUntilChanged()
				.Subscribe(i => flowBoxChild.Data[ForEachDataKeys.Index] = i, _ => { }, () => flowBoxChild.Destroy());
		});

		itemsObservable.Subscribe(_ =>
		{
			_flowBox.InvalidateSort();
			_flowBox.InvalidateFilter();
		});

		_flowBox.Events().Destroyed.Take(1).Subscribe(_ =>
		{
			_disableDragAndDrop.Dispose();
		});
	}

	private void ToggleDragSource(FlowBoxChild flowBoxChild, bool disabledDragAndDrop)
	{
		if (disabledDragAndDrop)
		{

		}
		else
		{

		}
	}

	private void EmitOrderingChanged()
	{
		var newOrdering = _flowBox.Children
			.Where(c => c.IsMapped)
			.Cast<FlowBoxChild>()
			.OrderBy(c => c.Data[ForEachDataKeys.Index])
			.Select(c => (TViewModel)c.Data[ForEachDataKeys.Model])
			.ToList();

		_orderingChangedSubject.OnNext(newOrdering);
	}

	private void OnDragEndInternal(FlowBoxChild flowBoxChild)
	{
		_dragIconImage.Hide();
		flowBoxChild.Child.Opacity = 1;
		EmitOrderingChanged();
	}

	private void OnDragMotion(GestureDrag dragGesture, FlowBoxChild flowBoxChild, int iconX, int iconY)
	{
		var lastPosition = _dragIconImage.Allocation;

		if (!_dragInitialized)
		{
			var size = (int) (flowBoxChild.Allocation.Height);
			_dragIconImage.ApplyViewModel(_dragImageViewModel, size, size);
			_dragIconImage.ShowAll();
			_dragBeginSubject.OnNext(flowBoxChild.Child as TWidget);
			_dragInitialized = true;
			flowBoxChild.Child.Opacity = 0;
			dragGesture.GetStartPoint(out var startX, out var startY);
			lastPosition.Location = new Point((int) startX, (int) startY);
		}

		flowBoxChild.TranslateCoordinates(_flowBox, iconX, iconY, out var x, out var y);
		var imageRect = new Rectangle(_flowBox.Allocation.Constrain(new Rectangle(new Point(x, y), lastPosition.Size)), lastPosition.Size);
		var children = _flowBox.Children.Where(c => c.IsMapped).Cast<FlowBoxChild>().OrderBy(c => c.Index).ToList();
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

		if (corner.HasValue && children.FirstOrDefault(c => c.Allocation.Contains(corner.Value)) is {} hovered)
		{
			children.Remove(flowBoxChild);
			children.Insert(hovered.Index, flowBoxChild);
			for (var i=0; i<children.Count; i++) children[i].Data[ForEachDataKeys.Index] = i;
			_flowBox.InvalidateSort();
		}

		_rootContainer.Move(_dragIconImage, imageRect.X, imageRect.Y);
	}

	private int SortByItemIndex(FlowBoxChild child1, FlowBoxChild child2)
	{
		var index1 = (int)child1.Data[ForEachDataKeys.Index];
		var index2 = (int)child2.Data[ForEachDataKeys.Index];
		return index1.CompareTo(index2);
	}
}
