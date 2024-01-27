using System.Reactive.Linq;
using Cairo;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ForEach;
using Glimpse.Common.Images;
using Gtk;
using ReactiveMarbles.ObservableEvents;
using Color = Cairo.Color;

namespace Glimpse.Taskbar.Components;

public class TaskbarGroupIcon : EventBox, IForEachDraggable
{
	private readonly TaskbarWindowPicker _taskbarWindowPicker;
	private SlotViewModel _currentViewModel;

	public IObservable<ImageViewModel> IconWhileDragging { get; }

	public TaskbarGroupIcon(IObservable<SlotViewModel> viewModel, TaskbarWindowPicker taskbarWindowPicker)
	{
		_taskbarWindowPicker = taskbarWindowPicker;
		Visible = false;
		Expand = false;
		Valign = Align.Fill;
		Halign = Align.Fill;
		AppPaintable = true;
		Visual = Screen.RgbaVisual;
		this.AddClass("taskbar__group-icon");

		var iconObservable = viewModel
			.Select(vm => vm.Icon)
			.DistinctUntilChanged()
			.CombineLatest(this.ObserveEvent(w => w.Events().SizeAllocated).DistinctUntilChanged(a => a.Allocation.Width))
			.Select(t => t.First)
			.TakeUntil(viewModel.TakeLast(1))
			.Replay(1);

		var image = new Image();
		Add(image);
		ShowAll();

		viewModel.Subscribe(vm => _currentViewModel = vm);
		viewModel.Select(vm => vm.DemandsAttention).DistinctUntilChanged().Subscribe(_ => QueueDraw());

		this.AppIcon(image, iconObservable, 26);
		this.ObserveEvent(w => w.Events().ButtonReleaseEvent).Subscribe(e => e.RetVal = true);
		IconWhileDragging = iconObservable.Select(i => i with { Image = (IGlimpseImage) image.Data["Big"] });

		iconObservable.Connect();
	}

	public void CloseWindowPicker()
	{
		_taskbarWindowPicker.ClosePopup();
	}

	protected override bool OnDrawn(Context cr)
	{
		if (_currentViewModel == null) return base.OnDrawn(cr);

		cr.Save();

		var w = Window.Width;
		var h = Window.Height;

		var demandsAttention = _currentViewModel.DemandsAttention;
		var backgroundAlpha = StateFlags.HasFlag(StateFlags.Prelight) ? 0.3
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
		return base.OnDrawn(cr);
	}
}
