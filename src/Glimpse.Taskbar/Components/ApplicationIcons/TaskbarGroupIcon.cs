using System.Reactive.Linq;
using Cairo;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ForEach;
using Glimpse.Common.Images;
using Glimpse.Taskbar.Components.WindowPicker;
using Gtk;
using ReactiveMarbles.ObservableEvents;
using Color = Cairo.Color;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

internal class TaskbarGroupIcon : IGlimpseFlowBoxItem
{
	private readonly EventBox _root;
	private readonly TaskbarWindowPicker _taskbarWindowPicker;
	private SlotViewModel _currentViewModel;

	public IObservable<ImageViewModel> IconWhileDragging { get; }
	public Widget Widget => _root;
	public SlotViewModel ViewModel => _currentViewModel;

	public TaskbarGroupIcon(IObservable<SlotViewModel> viewModel, TaskbarWindowPicker taskbarWindowPicker)
	{
		_taskbarWindowPicker = taskbarWindowPicker;

		_root = new EventBox();
		_root.Visible = false;
		_root.Expand = false;
		_root.Valign = Align.Fill;
		_root.Halign = Align.Fill;
		_root.AppPaintable = true;
		_root.Visual = _root.Screen.RgbaVisual;
		_root.AddClass("taskbar__group-icon");

		var iconObservable = viewModel
			.Select(vm => vm.Icon)
			.DistinctUntilChanged()
			.CombineLatest(_root.ObserveEvent(w => w.Events().SizeAllocated).DistinctUntilChanged(a => a.Allocation.Width))
			.Select(t => t.First)
			.TakeUntil(viewModel.TakeLast(1))
			.Replay(1)
			.AutoConnect();

		var image = new Image();
		_root.Add(image);
		_root.ShowAll();

		viewModel.Subscribe(vm => _currentViewModel = vm);
		viewModel.Select(vm => vm.DemandsAttention).DistinctUntilChanged().Subscribe(_ => _root.QueueDraw());
		viewModel.Select(vm => vm.Tasks.Count).DistinctUntilChanged().Subscribe(_ => _root.QueueDraw());

		_root.AppIcon(image, iconObservable, 26);
		_root.ObserveEvent(w => w.Events().ButtonReleaseEvent).Subscribe(e => e.RetVal = true);
		_root.ObserveEvent(w => w.Events().Drawn).Subscribe(e => e.RetVal = OnDrawn(e.Cr));
		IconWhileDragging = iconObservable.Select(i => i with { Image = (IGlimpseImage) image.Data["Big"] });
	}

	public void CloseWindowPicker()
	{
		_taskbarWindowPicker.ClosePopup();
	}

	private bool OnDrawn(Context cr)
	{
		if (_currentViewModel == null) return true;

		cr.Save();

		var w = _root.Window.Width;
		var h = _root.Window.Height;

		var demandsAttention = _currentViewModel.DemandsAttention;
		var backgroundAlpha = _root.StateFlags.HasFlag(StateFlags.Prelight) ? 0.3
			: _currentViewModel.Tasks.Count > 0 ? 0.1
			: 0;

		var backgroundColor = demandsAttention ? new Color(1, 1, 0, backgroundAlpha + 0.4) : new Color(1, 1, 1, backgroundAlpha);

		if (_currentViewModel.Tasks.Count == 0)
		{
			cr.Operator = Operator.Over;
			cr.SetSourceColor(backgroundColor);
			cr.RoundedRectangle(0, 0, w, h, 4);
			cr.Fill();
		}
		else if (_currentViewModel.Tasks.Count == 1)
		{
			cr.Operator = Operator.Over;
			cr.SetSourceColor(backgroundColor);
			cr.RoundedRectangle(0, 0, w, h, 4);
			cr.Fill();
		}
		else
		{
			var imageSurface = new ImageSurface(Format.ARGB32, w, h);
			var imageContext = new Context(imageSurface);

			imageContext.Operator = Operator.Over;
			imageContext.SetSourceRGBA(1, 0, 0, 1);
			imageContext.RoundedRectangle(0, 0, w-5, h, 4);
			imageContext.Fill();

			imageContext.Operator = Operator.Out;
			imageContext.SetSourceRGBA(0, 1, 0, 1);
			imageContext.RoundedRectangle(0, 0, w-2, h, 4);
			imageContext.Fill();

			imageContext.Operator = Operator.Out;
			imageContext.SetSourceColor(backgroundColor);
			imageContext.RoundedRectangle(0, 0, w, h, 4);
			imageContext.Fill();

			cr.SetSourceSurface(imageSurface, 0, 0);
			cr.Paint();

			imageSurface.Dispose();
			imageContext.Dispose();
		}

		cr.Restore();
		return true;
	}
}
