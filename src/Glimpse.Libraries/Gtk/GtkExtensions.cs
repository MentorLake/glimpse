using System.Drawing;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Glimpse.Libraries.Images;
using Glimpse.Libraries.System;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.X11;
using MentorLake.cairo;
using MentorLake.Gdk;
using MentorLake.GdkPixbuf;
using MentorLake.GdkX11;
using MentorLake.GLib;
using MentorLake.Gtk;
using MentorLake.Gtk3;

namespace Glimpse.Libraries.Gtk;

public static class GtkExtensions
{
	public static Rectangle TrimSides(this Rectangle r, int amount)
	{
		var copy = r;
		copy.Width -= amount * 2;
		copy.X += amount;
		copy.Height -= amount * 2;
		copy.Y += amount;
		return copy;
	}

	public static Point LowerRight(this Rectangle r)
	{
		return new Point(r.Right, r.Bottom);
	}

	public static Point LowerLeft(this Rectangle r)
	{
		return new Point(r.X, r.Bottom);
	}

	public static Point UpperRight(this Rectangle r)
	{
		return new Point(r.Right, r.Y);
	}

	public static Point UpperLeft(this Rectangle r)
	{
		return new Point(r.X, r.Y);
	}

	public static Point Constrain(this Rectangle container, Rectangle item)
	{
		var x = item.X;
		var y = item.Y;

		if (item.X + item.Width >= container.Width) x = container.Width - item.Width;
		if (item.Left <= 0) x = 0;
		if (item.Top <= 0) y = 0;
		if (item.Y + item.Height >= container.Height) y = container.Height - item.Height;

		return new Point(x, y);
	}

	public static Rectangle ToDrawingRectangle(this GdkRectangle rect)
	{
		return new Rectangle() { Height = rect.height, Width = rect.width, X = rect.x, Y = rect.y };
	}

	public static Rectangle GetGeometryRect(this GdkMonitorHandle monitor)
	{
		monitor.GetGeometry(out var geometry);
		return geometry.ToDrawingRectangle();
	}

	public static Rectangle GetWorkAreaRect(this GdkMonitorHandle monitor)
	{
		monitor.GetWorkarea(out var geometry);
		return geometry.ToDrawingRectangle();
	}

	private static XWindowRef ToWindowRef(this GdkWindowHandle window)
	{
		return new XWindowRef()
		{
			Display = (ulong) window.GetDisplay().GetXdisplay().DangerousGetHandle(),
			Window = window.GetXid().Value
		};
	}

	public static void ReserveSpace(this GtkWindowHandle window, Rectangle monitor, Rectangle workArea)
	{
		var windowRef = window.GetWindow().ToWindowRef();
		var windowAllocation = window.GetAllocationRect();
		var cardinalTypeAtom = XLib.XInternAtom(windowRef.Display, "CARDINAL", false);

		var strutAtom = XLib.XInternAtom(windowRef.Display, "_NET_WM_STRUT", false);
		var reservedSpace = new long[] { 0, 0, 0, windowAllocation.Height }.SelectMany(BitConverter.GetBytes).ToArray();
		XLib.XChangeProperty(windowRef.Display, windowRef.Window, strutAtom, cardinalTypeAtom, 32, (int) GdkPropMode.GDK_PROP_MODE_REPLACE, reservedSpace, reservedSpace.Length);

		var strutPartialAtom = XLib.XInternAtom(windowRef.Display, "_NET_WM_STRUT_PARTIAL", false);
		var reservedSpaceLong = new long[] { 0, 0, 0, windowAllocation.Height, 0, 0, 0, 0, 0, 0, workArea.Left, workArea.Left + monitor.Width - 1 }.SelectMany(BitConverter.GetBytes).ToArray();
		XLib.XChangeProperty(windowRef.Display, windowRef.Window, strutPartialAtom, cardinalTypeAtom, 32, (int) GdkPropMode.GDK_PROP_MODE_REPLACE, reservedSpaceLong, reservedSpaceLong.Length);
	}

	public static IEnumerable<GdkMonitorHandle> GetMonitors(this GdkDisplayHandle display)
	{
		for (var i = 0; i < display.GetNMonitors(); i++)
		{
			yield return display.GetMonitor(i);
		}
	}

	public static T ToHandle<T>(this IntPtr ptr) where T : SafeHandle, new()
	{
		var handle = new T();
		Marshal.InitHandle(handle, ptr);
		return handle;
	}

	public static List<T> ChildList<T>(this GtkContainerHandle container) where T : SafeHandle, new()
	{
		var children = container.GetChildren();
		var result = children.ToList<T>();
		GList.Free(children);
		return result;
	}

	public static List<T> ToList<T>(this GListHandle gListHandle) where T : SafeHandle, new()
	{
		var length = GList.Length(gListHandle);
		var result = new List<T>((int) length);

		for (uint i = 0; i < length; i++)
		{
			var ptr = GList.NthData(gListHandle, i);
			var handle = new T();
			Marshal.InitHandle(handle, ptr);
			result.Add(handle);
		}

		return result;
	}

	public static void FreeFull(this GListHandle gListHandle)
	{
		GList.FreeFull(gListHandle, _ => { });
	}

	public static void RemoveAllChildren(this GtkContainerHandle widget)
	{
		var children = widget.GetChildren();
		var widgets = children.ToList<GtkWidgetHandle>();
		widgets.ForEach(w => widget.Remove(w));
		GList.Free(children);
	}

	private static bool ContainsPoint(this GtkWidgetHandle widget, int px, int py)
	{
		if (!widget.IsVisible()) return false;
		widget.GetWindow().GetGeometry(out _, out _, out var width, out var height);
		widget.GetWindow().GetRootCoords(0, 0, out var widgetRootX, out var widgetRootY);
		return px >= widgetRootX && py >= widgetRootY && px < widgetRootX + width && py < widgetRootY + height;
	}

	public static bool IsPointerInside(this GtkWidgetHandle widget)
	{
		widget.GetDisplay().GetPointer(out _, out var px, out var py, out _);
		return widget.ContainsPoint(px, py);
	}

	public static GtkImageHandle BindViewModel(this GtkImageHandle image, IObservable<ImageViewModel> imageViewModel, int size, bool useMissingImage = true)
	{
		image.BindViewModel(imageViewModel, size, size, useMissingImage);
		return image;
	}

	public static GtkSwitchHandle BindViewModel(this GtkSwitchHandle sw, IObservable<bool> viewModelObs)
	{
		viewModelObs.TakeUntilDestroyed(sw).SubscribeDebug(val => sw.SetState(val));
		return sw;
	}

	public static readonly string MissingIconName = Guid.NewGuid().ToString();

	public static void BindViewModel(this GtkImageHandle image, IObservable<ImageViewModel> imageViewModel, int width, int height, bool useMissingImage = true)
	{
		imageViewModel.TakeUntilDestroyed(image).SubscribeDebug(vm =>
		{
			image.ApplyViewModel(vm, width, height, useMissingImage);
		});
	}

	public static void ApplyViewModel(this GtkImageHandle image, ImageViewModel vm, int width, int height, bool useMissingImage = true)
	{
		if (vm.Image != null)
		{
			var scaledIcon = vm.Image.ScaleToFit(width, height);
			image.SetFromPixbuf(scaledIcon);
		}
		else if (vm.IconNameOrPath.Or("").StartsWith("/"))
		{
			using var icon = GdkPixbufFactory.From(vm.IconNameOrPath);
			var scaledPixbuf = icon.ScaleToFit(width, height);
			image.SetFromPixbuf(scaledPixbuf);
		}
		else
		{
			image.SetFromIconName(string.IsNullOrEmpty(vm.IconNameOrPath) ? (useMissingImage ? MissingIconName : "") : vm.IconNameOrPath, GtkIconSize.GTK_ICON_SIZE_LARGE_TOOLBAR);
			image.SetPixelSize(width);
		}
	}

	public static T Prop<T>(this T widget, Action<T> action) where T : GtkWidgetHandle
	{
		action(widget);
		return widget;
	}

	private static void AddIconToImage(GtkImageHandle image, GdkPixbufHandle icon, int size, bool disposeOriginal = false)
	{
		var small = icon.ScaleSimple(size - 6, size - 6, GdkInterpType.GDK_INTERP_BILINEAR);
		var big = icon.ScaleSimple(size, size, GdkInterpType.GDK_INTERP_BILINEAR);

		image.Signal_Destroy().Take(1).SubscribeDebug(_ =>
		{
			if (disposeOriginal) icon.Dispose();
		});

		image.SetFromPixbuf(icon);
		image.SetManagedData("Small", small);
		image.SetManagedData("Big", big);
	}

	public static GdkPixbufHandle LoadIcon(string name, int size)
	{
		if (name.StartsWith("/"))
		{
			return GdkPixbufHandle.NewFromFileAtSize(name, size, size);
		}

		try
		{
			return GtkIconThemeHandle.GetDefault().LoadIconForScale(name, size, 1, GtkIconLookupFlags.GTK_ICON_LOOKUP_USE_BUILTIN);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return name != "image-missing" ? LoadIcon("image-missing", size) : null;
		}
	}

	public static void AppIcon(this GtkWidgetHandle widget, GtkImageHandle image, IObservable<ImageViewModel> iconObservable, int size)
	{
		iconObservable.TakeUntilDestroyed(image).SubscribeDebug(vm =>
		{
			AddIconToImage(image, vm.Image ?? LoadIcon(vm.IconNameOrPath, size), size, vm.Image == null);
		});

		widget.AddButtonStates();
		widget.ObserveEvent(w => w.Signal_EnterNotifyEvent()).SubscribeDebug(_ => widget.QueueDraw());
		widget.ObserveEvent(w => w.Signal_LeaveNotifyEvent()).SubscribeDebug(_ => widget.QueueDraw());
		widget.ObserveEvent(w => w.Signal_ButtonPressEvent()).SubscribeDebug(_ => widget.QueueDraw());
		widget.ObserveEvent(w => w.Signal_ButtonPressEvent()).WithLatestFrom(iconObservable).SubscribeDebug(_ =>
		{
			if (image.GetPixbuf() == null) image.SetPixelSize(size - 6);
			else image.SetFromPixbuf(image.GetManagedData<GdkPixbufHandle>("Small"));
		});
		widget.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).WithLatestFrom(iconObservable).SubscribeDebug(_ =>
		{
			if (image.GetPixbuf() == null) image.SetPixelSize(size);
			else image.SetFromPixbuf(image.GetManagedData<GdkPixbufHandle>("Big"));
		});
		widget.ObserveEvent(w => w.Signal_LeaveNotifyEvent()).WithLatestFrom(iconObservable).SubscribeDebug(_ =>
		{
			if (image.GetPixbuf() == null) image.SetPixelSize(size);
			else image.SetFromPixbuf(image.GetManagedData<GdkPixbufHandle>("Big"));
		});
	}

	public static TWidget AddButtonStates<TWidget>(this TWidget widget) where TWidget : GtkWidgetHandle
	{
		var genericWidget = widget as GtkWidgetHandle;
		widget.AddEvents((int)(GdkEventMask.GDK_BUTTON_PRESS_MASK | GdkEventMask.GDK_BUTTON_RELEASE_MASK | GdkEventMask.GDK_ENTER_NOTIFY_MASK | GdkEventMask.GDK_LEAVE_NOTIFY_MASK));
		genericWidget.ObserveEvent(w => w.Signal_EnterNotifyEvent()).SubscribeDebug(_ => widget.SetStateFlags(GtkStateFlags.GTK_STATE_FLAG_PRELIGHT, true));
		genericWidget.ObserveEvent(w => w.Signal_LeaveNotifyEvent()).Where(_ => !widget.IsPointerInside()).SubscribeDebug(_ => widget.SetStateFlags(GtkStateFlags.GTK_STATE_FLAG_NORMAL, true));
		genericWidget.ObserveEvent(w => w.Signal_ButtonPressEvent()).SubscribeDebug(_ => widget.SetStateFlags(GtkStateFlags.GTK_STATE_FLAG_ACTIVE, true));
		genericWidget.ObserveEvent(w => w.Signal_ButtonReleaseEvent()).SubscribeDebug(_ => widget.SetStateFlags(GtkStateFlags.GTK_STATE_FLAG_PRELIGHT, true));
		return widget;
	}

	public static Rectangle GetAllocationRect(this GtkWidgetHandle widget)
	{
		widget.GetAllocation(out var allocation);
		var a = allocation.Value;
		return new Rectangle(new Point(a.x, a.y), new Size(a.width, a.height));
	}

	public static void CenterAbove(this GtkWindowHandle window, GtkWidgetHandle widget)
	{
		if (!window.IsVisible()) return;

		widget.GetWindow().GetRootCoords(0, 0, out var x, out var y);

		var windowX = x - window.GetWindow().GetWidth() / 2 + widget.GetWindow().GetWidth() / 2;
		var windowY = y - window.GetWindow().GetHeight() - 16;

		window.Move(windowX, windowY);
	}

	public static void CenterOnScreenAtBottom(this GtkWindowHandle window, GdkMonitorHandle monitor)
	{
		if (!window.IsVisible()) return;

		monitor.GetGeometry(out var monitorGeometry);
		var windowAllocation = window.GetAllocationRect();
		var displayId = XLib.XOpenDisplay(0);
		var windowId = XLib.XDefaultRootWindow(displayId);
		var windowRef = new XWindowRef() { Display = displayId, Window = windowId };

		var workAreaAtom = XLib.XInternAtom(displayId, "_NET_WORKAREA", false);
		var workArea = windowRef.GetULongArray(workAreaAtom);
		var fullWorkArea = new Rectangle() { X = 0, Y = 0, Width = (int) workArea[2], Height = (int) workArea[3] };
		var monitorWorkArea = Rectangle.Intersect(fullWorkArea, monitorGeometry.ToDrawingRectangle());
		var windowX = monitorWorkArea.Left + monitorGeometry.width / 2 - windowAllocation.Width / 2;
		var windowY = monitorWorkArea.Height - windowAllocation.Height - 16;
		window.Move(windowX, windowY);
	}

	public static IObservable<T> TakeUntilDestroyed<T>(this IObservable<T> obs, GtkWidgetHandle source)
	{
		return obs.TakeUntil(source.Signal_Destroy().Take(1));
	}

	public static IObservable<T> ObserveEvent<T>(this GtkWidgetHandle widget, IObservable<T> obs)
	{
		return obs.TakeUntilDestroyed(widget);
	}

	public static IObservable<T> ObserveEvent<TWidget, T>(this TWidget widget, Func<TWidget, IObservable<T>> f) where TWidget : GtkWidgetHandle
	{
		return f(widget).TakeUntilDestroyed(widget);
	}

	public static TWidget ObserveEvent<TWidget, T>(this TWidget widget, Func<TWidget, IObservable<T>> f, Action<T> subscribeAction) where TWidget : GtkWidgetHandle
	{
		f(widget).TakeUntilDestroyed(widget).SubscribeDebug(subscribeAction);
		return widget;
	}

	public static IObservable<T> ObserveEvent<TWidget, T>(this TWidget widget, IObservable<T> obs) where TWidget : GtkWidgetHandle
	{
		return obs.TakeUntilDestroyed(widget);
	}

	public static T AddClass<T>(this T widget, params string[] classes) where T : GtkWidgetHandle
	{
		foreach (var c in classes) widget.GetStyleContext().AddClass(c);
		return widget;
	}

	public static T RemoveClass<T>(this T widget, params string[] classes) where T : GtkWidgetHandle
	{
		foreach (var c in classes) widget.GetStyleContext().RemoveClass(c);
		return widget;
	}

	public static T AddMany<T>(this T widget, params GtkWidgetHandle[] children) where T : GtkContainerHandle
	{
		foreach (var c in children) widget.Add(c);
		return widget;
	}

	public static void RoundedRectangle(this cairo_tHandle cr, int x, int y, int width, int height, int cornerRadius)
	{
		cr.RoundedRectangle(x, y, width, height, cornerRadius, cornerRadius, cornerRadius, cornerRadius);
	}

	private static void RoundedRectangle(this cairo_tHandle cr, int x, int y, int width, int height, int upperRightRadius, int lowerRightRadius, int lowerLeftRadius, int upperLeftRadius)
	{
		var degrees = Math.PI / 180.0;

		cairoGlobalFunctions.NewSubPath(cr);
		cairoGlobalFunctions.Arc(cr, x + width - upperRightRadius, y + upperRightRadius, upperRightRadius, -90 * degrees, 0 * degrees);
		cairoGlobalFunctions.Arc(cr, x + width - lowerRightRadius, y + height - lowerRightRadius, lowerRightRadius, 0 * degrees, 90 * degrees);
		cairoGlobalFunctions.Arc(cr, x + lowerLeftRadius, y + height - lowerLeftRadius, lowerLeftRadius, 90 * degrees, 180 * degrees);
		cairoGlobalFunctions.Arc(cr, x + upperLeftRadius, y + upperLeftRadius, upperLeftRadius, 180 * degrees, 270 * degrees);
		cairoGlobalFunctions.ClosePath(cr);
	}

	public static void SetIndex(this GtkFlowBoxChildHandle child, int index)
	{
		child.SetManagedData(IndexString, index);
	}

	private const string IndexString = "Index";

	public static int GetIndex(this GtkFlowBoxChildHandle child)
	{
		return child.GetManagedData<int>(IndexString);
	}
}
