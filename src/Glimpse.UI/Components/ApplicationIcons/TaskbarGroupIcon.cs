using Glimpse.Libraries.Gtk;
using Glimpse.Services.Taskbar;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ForEach;
using Glimpse.UI.Components.WindowPicker;
using MentorLake.cairo;
using MentorLake.Gdk;
using MentorLake.GdkPixbuf;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.ApplicationIcons;

internal class TaskbarGroupIcon : IGlimpseFlowBoxItem
{
	private bool _demandsAttention;
	private int _taskCount;

	private readonly TaskbarWindowPicker _taskbarWindowPicker;
	private readonly AppIcon<SlotContextMenuItemViewModel> _appIcon;

	public GtkWidgetHandle Widget => _appIcon.Widget;
	public GdkPixbufHandle DragIcon => _appIcon.DragIcon;
	public SlotRef SlotRef { get; private set; }
	public IObservable<SlotContextMenuItemViewModel> ContextMenuItemActivated => _appIcon.ContextMenuItemActivated;

	public TaskbarGroupIcon(TaskbarWindowPicker taskbarWindowPicker, IObservable<AppIconViewModel<SlotContextMenuItemViewModel>> viewModelObs)
	{
		_taskbarWindowPicker = taskbarWindowPicker;
		_appIcon = new AppIcon<SlotContextMenuItemViewModel>("", 26, viewModelObs);
		_appIcon.Widget
			.SetVisible(false)
			.SetValign(GtkAlign.GTK_ALIGN_FILL)
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.AddClass("taskbar__group-icon")
			.ObserveEvent(w => w.Signal_Draw()).Subscribe(e => e.ReturnValue = OnDrawn(e.Cr));
	}

	public void CloseWindowPicker()
	{
		_taskbarWindowPicker.ClosePopup();
	}

	public void UpdateSlotRef(SlotRef slotRef)
	{
		SlotRef = slotRef;
	}

	public void UpdateDemandsAttention(bool val)
	{
		_demandsAttention = val;
		_appIcon.Widget.QueueDraw();
	}

	public void UpdateTaskCount(int taskCount)
	{
		_taskCount = taskCount;
		_appIcon.Widget.QueueDraw();
	}

	private bool OnDrawn(cairo_tHandle cr)
	{
		cairoGlobalFunctions.Save(cr);

		var w = _appIcon.Widget.GetWindow().GetWidth();
		var h = _appIcon.Widget.GetWindow().GetHeight();

		var backgroundBlue = 1;
		var backgroundAlpha = _appIcon.Widget.GetStateFlags().HasFlag(GtkStateFlags.GTK_STATE_FLAG_PRELIGHT) ? 0.3
			: _taskCount > 0 ? 0.1
			: 0;

		if (_demandsAttention)
		{
			backgroundAlpha += 0.4;
			backgroundBlue = 0;
		}

		if (_taskCount == 0)
		{
			cairoGlobalFunctions.SetOperator(cr, cairo_operator_t.CAIRO_OPERATOR_OVER);
			cairoGlobalFunctions.SetSourceRgba(cr, 1, 1, backgroundBlue, backgroundAlpha);
			cr.RoundedRectangle(0, 0, w, h, 4);
			cairoGlobalFunctions.Fill(cr);
		}
		else if (_taskCount == 1)
		{
			cairoGlobalFunctions.SetOperator(cr, cairo_operator_t.CAIRO_OPERATOR_OVER);
			cairoGlobalFunctions.SetSourceRgba(cr, 1, 1, backgroundBlue, backgroundAlpha);
			cr.RoundedRectangle(0, 0, w, h, 4);
			cairoGlobalFunctions.Fill(cr);
		}
		else
		{
			var imageSurface = cairoGlobalFunctions.ImageSurfaceCreate(cairo_format_t.CAIRO_FORMAT_ARGB32, w, h);
			var imageContext = cairoGlobalFunctions.Create(imageSurface);

			cairoGlobalFunctions.SetOperator(imageContext, cairo_operator_t.CAIRO_OPERATOR_OVER);
			cairoGlobalFunctions.SetSourceRgba(imageContext, 1, 0, 0, 1);
			imageContext.RoundedRectangle(0, 0, w-5, h, 4);
			cairoGlobalFunctions.Fill(imageContext);

			cairoGlobalFunctions.SetOperator(imageContext, cairo_operator_t.CAIRO_OPERATOR_OUT);
			cairoGlobalFunctions.SetSourceRgba(imageContext, 0, 1, 0, 1);
			imageContext.RoundedRectangle(0, 0, w-2, h, 4);
			cairoGlobalFunctions.Fill(imageContext);

			cairoGlobalFunctions.SetOperator(imageContext, cairo_operator_t.CAIRO_OPERATOR_OUT);
			cairoGlobalFunctions.SetSourceRgba(imageContext, 1, 1, backgroundBlue, backgroundAlpha);
			imageContext.RoundedRectangle(0, 0, w, h, 4);
			cairoGlobalFunctions.Fill(imageContext);

			cairoGlobalFunctions.SetSourceSurface(cr, imageSurface, 0, 0);
			cairoGlobalFunctions.Paint(cr);

			cairoGlobalFunctions.SurfaceDestroy(imageSurface);
			cairoGlobalFunctions.Destroy(imageContext);
		}

		cairoGlobalFunctions.Restore(cr);
		return true;
	}
}
