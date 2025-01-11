using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg;
using Glimpse.UI.Components.ApplicationIcons;
using Glimpse.UI.Components.Shared;
using MentorLake.Gdk;
using MentorLake.Gtk;
using MentorLake.Pango;

namespace Glimpse.UI.Components.WindowPicker;

internal class TaskbarWindowPicker
{
	private readonly Subject<IWindowRef> _previewWindowClicked = new();
	private readonly Subject<IWindowRef> _closeWindow = new();
	public GtkWindowHandle Widget { get; }

	public TaskbarWindowPicker(IObservable<SlotViewModel> viewModelObservable)
	{
		Widget = GtkWindowHandle.New(GtkWindowType.GTK_WINDOW_POPUP);
		Widget.SetSkipPagerHint(true);
		Widget.SetSkipTaskbarHint(true);
		Widget.SetDecorated(false);
		Widget.SetResizable(false);
		Widget.SetCanFocus(true);
		Widget.SetTypeHint(GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_DIALOG);
		Widget.SetVisual(Widget.GetScreen().GetRgbaVisual());
		Widget.SetKeepAbove(true);
		Widget.Signal_DeleteEvent().Subscribe(e => e.ReturnValue = true);

		var layout = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		Widget.Add(layout);
		Widget.ObserveEvent(w => w.Signal_FocusOutEvent()).Subscribe(_ => ClosePopup());

		viewModelObservable.Select(vm => vm.Tasks).UnbundleMany(t => t.WindowRef.Id).RemoveIndex().Subscribe(taskObservable =>
		{
			var preview = CreateAppPreview(taskObservable);
			layout.Add(preview);
			taskObservable.TakeLast(1).Subscribe(_ => preview.Destroy());
		});

		Widget.Signal_Destroy().Take(1).Subscribe(_ =>
		{
			_previewWindowClicked.OnCompleted();
			_closeWindow.OnCompleted();
		});
	}

	public IObservable<IWindowRef> PreviewWindowClicked => _previewWindowClicked;
	public IObservable<IWindowRef> CloseWindow => _closeWindow;

	public void ClosePopup()
	{
		Widget.SetVisible(false);
	}

	public void Popup()
	{
		Widget.ShowAll();
	}

	private GtkWidgetHandle CreateAppPreview(IObservable<WindowViewModel> taskObservable)
	{
		var appName = GtkLabelHandle.New("")
			.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END)
			.SetJustify(GtkJustification.GTK_JUSTIFY_LEFT)
			.SetHalign(GtkAlign.GTK_ALIGN_START)
			.SetMaxWidthChars(15)
			.SetHexpand(true)
			.SetVexpand(true)
			.AddClass("window-picker__app-name");

		var appIcon = GtkImageHandle.New()
			.SetHalign(GtkAlign.GTK_ALIGN_START)
			.AddClass("window-picker__app-icon");

		var closeIconBox = GtkButtonHandle.New()
			.SetHalign(GtkAlign.GTK_ALIGN_END)
			.AddClass("window-picker__app-close-button")
			.AddMany(GtkImageHandle.New()
				.SetFromIconName("window-close-symbolic", GtkIconSize.GTK_ICON_SIZE_BUTTON)
				.SetPixelSize(12));

		var screenshotImage = GtkImageHandle.New()
			.SetHalign(GtkAlign.GTK_ALIGN_CENTER)
			.AddClass("window-picker__screenshot");

		var grid = GtkGridHandle.New();
		grid.Attach(appIcon, 0, 0, 1, 1);
		grid.AttachNextTo(appName, appIcon, GtkPositionType.GTK_POS_RIGHT, 1, 1);
		grid.AttachNextTo(closeIconBox, appName, GtkPositionType.GTK_POS_RIGHT, 1, 1);
		grid.Attach(screenshotImage, 0, 1, 3, 1);
		grid.AddClass("window-picker__app");

		var appPreview = GtkEventBoxHandle.New()
			.AddClass("window-picker__app-events")
			.AddMany(grid)
			.AddButtonStates();

		taskObservable.Select(t => t.Title).DistinctUntilChanged().Subscribe(t => appName.SetText(t));
		taskObservable.Select(t => t.Icon).DistinctUntilChanged().Subscribe(i => appIcon.SetFromPixbuf(IconManager.GetDefault().GetIcon(i, 18).Image));
		closeIconBox.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).WithLatestFrom(taskObservable).Subscribe(t => _closeWindow.OnNext(t.Second.WindowRef));
		screenshotImage.BindViewModel(taskObservable.Select(s => s.Screenshot).DistinctUntilChanged(), 200, 100);
		appPreview.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).WithLatestFrom(taskObservable).Subscribe(t => _previewWindowClicked.OnNext(t.Second.WindowRef));

		return appPreview;
	}
}
