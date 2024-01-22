using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gdk;
using Glimpse.Common.System.Reactive;
using Glimpse.UI;
using Glimpse.UI.Components.Shared;
using Glimpse.Xorg;
using Gtk;
using Pango;
using ReactiveMarbles.ObservableEvents;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace Glimpse.Taskbar.Components;

public class TaskbarWindowPicker : Window
{
	private readonly Subject<IWindowRef> _previewWindowClicked = new();
	private readonly Subject<IWindowRef> _closeWindow = new();

	public TaskbarWindowPicker(IObservable<SlotViewModel> viewModelObservable) : base(WindowType.Popup)
	{
		SkipPagerHint = true;
		SkipTaskbarHint = true;
		Decorated = false;
		Resizable = false;
		CanFocus = true;
		TypeHint = WindowTypeHint.Dialog;
		Visual = Screen.RgbaVisual;
		KeepAbove = true;

		this.Events().DeleteEvent.Subscribe(e => e.RetVal = true);

		var layout = new Box(Orientation.Horizontal, 0);
		Add(layout);
		this.ObserveEvent(w => ObservableGeneratorExtensions.Events((TaskbarWindowPicker)w).FocusOutEvent).Subscribe(_ => ClosePopup());

		viewModelObservable.Select(vm => vm.Tasks).UnbundleMany(t => t.WindowRef.Id).RemoveIndex().Subscribe(taskObservable =>
		{
			var preview = CreateAppPreview(taskObservable);
			layout.Add(preview);
			taskObservable.TakeLast(1).Subscribe(_ => preview.Destroy());
		});

		this.Events().Destroyed.Take(1).Subscribe(_ =>
		{
			_previewWindowClicked.OnCompleted();
			_closeWindow.OnCompleted();
		});
	}

	public IObservable<IWindowRef> PreviewWindowClicked => _previewWindowClicked;
	public IObservable<IWindowRef> CloseWindow => _closeWindow;

	public void ClosePopup()
	{
		Visible = false;
	}

	public void Popup()
	{
		ShowAll();
	}

	private Widget CreateAppPreview(IObservable<WindowViewModel> taskObservable)
	{
		var appName = new Label()
			{
				Ellipsize = EllipsizeMode.End,
				Justify = Justification.Left,
				Halign = Align.Start,
				MaxWidthChars = 15,
				Expand = true
			}
			.AddClass("window-picker__app-name");

		var appIcon = new Image() { Halign = Align.Start }
			.AddClass("window-picker__app-icon");

		var closeIconBox = new Button() { Halign = Align.End }
			.AddClass("window-picker__app-close-button")
			.AddMany(new Image() { IconName = "window-close-symbolic", PixelSize = 12 });

		var screenshotImage = new Image() { Halign = Align.Center }
			.AddClass("window-picker__screenshot");

		var grid = new Grid();
		grid.Attach(appIcon, 0, 0, 1, 1);
		grid.AttachNextTo(appName, appIcon, PositionType.Right, 1, 1);
		grid.AttachNextTo(closeIconBox, appName, PositionType.Right, 1, 1);
		grid.Attach(screenshotImage, 0, 1, 3, 1);
		grid.AddClass("window-picker__app");

		var appPreview = new EventBox()
			.AddClass("window-picker__app-events")
			.AddMany(grid)
			.AddButtonStates();

		taskObservable.Select(t => t.Title).DistinctUntilChanged().Subscribe(t => appName.Text = t);
		appIcon.BindViewModel(taskObservable.Select(t => t.Icon).DistinctUntilChanged(), ThemeConstants.MenuItemIconSize);
		closeIconBox.ObserveButtonRelease().WithLatestFrom(taskObservable).Subscribe(t => _closeWindow.OnNext(t.Second.WindowRef));
		screenshotImage.BindViewModel(taskObservable.Select(s => s.Screenshot).DistinctUntilChanged(), 200, 100);
		appPreview.ObserveButtonRelease().WithLatestFrom(taskObservable).Subscribe(t => _previewWindowClicked.OnNext(t.Second.WindowRef));

		return appPreview;
	}
}
