using System.Reactive.Disposables;
using System.Reactive.Linq;
using Cairo;
using Gdk;
using Glimpse.Common.Images;
using Gtk;
using ReactiveMarbles.ObservableEvents;
using Monitor = Gdk.Monitor;
using Rectangle = Gdk.Rectangle;
using Window = Gtk.Window;

namespace Glimpse.Common.Gtk;

public static class GtkExtensions
{
	public static IEnumerable<Monitor> GetMonitors(this Display display)
	{
		for (var i = 0; i < display.NMonitors; i++)
		{
			yield return display.GetMonitor(i);
		}
	}

	public static void RemoveAllChildren(this Container widget)
	{
		var widgets = widget.Children.ToList();
		widgets.ForEach(w => w.Destroy());
	}

	private static bool ContainsPoint(this Widget widget, int px, int py)
	{
		if (!widget.IsVisible) return false;
		widget.Window.GetGeometry(out _, out _, out var width, out var height);
		widget.Window.GetRootCoords(0, 0, out var widgetRootX, out var widgetRootY);
		return px >= widgetRootX && py >= widgetRootY && px < widgetRootX + width && py < widgetRootY + height;
	}

	public static bool IsPointerInside(this Widget widget)
	{
		widget.Display.GetPointer(out var px, out var py);
		return widget.ContainsPoint(px, py);
	}

	public static Image BindViewModel(this Image image, IObservable<ImageViewModel> imageViewModel, int size, bool useMissingImage = true)
	{
		image.BindViewModel(imageViewModel, size, size, useMissingImage);
		return image;
	}

	public static Switch BindViewModel(this Switch sw, IObservable<bool> viewModelObs)
	{
		viewModelObs.TakeUntilDestroyed(sw).Subscribe(val => sw.State = val);
		return sw;
	}

	public static TWidget Signal<TWidget>(this TWidget widget, string name, Action<TWidget, EventArgs> handler)
		where TWidget : Widget
	{

		widget.AddSignalHandler<EventArgs>(name).Subscribe(e => handler(widget, e));
		return widget;
	}

	private static IObservable<T> AddSignalHandler<T>(this Widget widget, string name) where T : EventArgs
	{
		return Observable.Create((IObserver<T> obs) =>
		{
			widget.AddSignalHandler(name, (object sender, T eventArgs) => obs.OnNext(eventArgs));
			widget.Events().Destroyed.Take(1).Subscribe(_ => obs.OnCompleted());
			return Disposable.Create(obs.OnCompleted);
		});
	}

	public static readonly string MissingIconName = Guid.NewGuid().ToString();

	public static void BindViewModel(this Image image, IObservable<ImageViewModel> imageViewModel, int width, int height, bool useMissingImage = true)
	{
		imageViewModel.Subscribe(vm =>
		{
			if (vm.Image != null)
			{
				image.Pixbuf = vm.Image.ScaleToFit(width, height).Pixbuf;
			}
			else if (vm.IconNameOrPath.StartsWith("/"))
			{
				image.Pixbuf = new Pixbuf(vm.IconNameOrPath).ScaleToFit(width, height);
			}
			else
			{
				image.SetFromIconName(string.IsNullOrEmpty(vm.IconNameOrPath) ? (useMissingImage ? MissingIconName : "") : vm.IconNameOrPath, IconSize.LargeToolbar);
				image.PixelSize = width;
			}
		});
	}

	public static T Prop<T>(this T widget, Action<T> action) where T : Widget
	{
		action(widget);
		return widget;
	}

	public static void AppIcon(this Widget widget, Image image, IObservable<ImageViewModel> iconObservable, int size)
	{
		iconObservable.Subscribe(vm =>
		{
			if (vm.Image != null)
			{
				image.Data["Small"] = vm.Image.Scale(size - 6);
				image.Data["Big"] = vm.Image.Scale(size);
			}
			else if (vm.IconNameOrPath.StartsWith("/"))
			{
				var glimpseImage = GlimpseImageFactory.From(new Pixbuf(vm.IconNameOrPath, size, size));
				image.Pixbuf = glimpseImage.Pixbuf;
				image.Data["Small"] = glimpseImage.Scale(size - 6);
				image.Data["Big"] = glimpseImage;
			}
		});

		image.BindViewModel(iconObservable, size);
		widget.AddButtonStates();
		widget.ObserveEvent(w => w.Events().EnterNotifyEvent).Subscribe(_ => widget.QueueDraw());
		widget.ObserveEvent(w => w.Events().LeaveNotifyEvent).Subscribe(_ => widget.QueueDraw());
		widget.ObserveEvent(w => w.Events().ButtonPressEvent).Subscribe(_ => widget.QueueDraw());
		widget.ObserveEvent(w => w.Events().ButtonPressEvent).WithLatestFrom(iconObservable).Subscribe(_ =>
		{
			if (image.Pixbuf == null) image.PixelSize = size - 6;
			else image.Pixbuf = ((GtkGlimpseImage)image.Data["Small"])?.Pixbuf;
		});
		widget.ObserveEvent(w => w.Events().ButtonReleaseEvent).WithLatestFrom(iconObservable).Subscribe(_ =>
		{
			if (image.Pixbuf == null) image.PixelSize = size;
			else image.Pixbuf = ((GtkGlimpseImage)image.Data["Big"])?.Pixbuf;
		});
		widget.ObserveEvent(w => w.Events().LeaveNotifyEvent).WithLatestFrom(iconObservable).Subscribe(_ =>
		{
			if (image.Pixbuf == null) image.PixelSize = size;
			else image.Pixbuf = ((GtkGlimpseImage)image.Data["Big"])?.Pixbuf;
		});
	}

	public static IObservable<ButtonReleaseEventArgs> ObserveButtonRelease(this Widget widget)
	{
		return widget.ObserveEvent(w => w.Events().ButtonReleaseEvent).TakeUntilDestroyed(widget);
	}

	public static TWidget AddButtonStates<TWidget>(this TWidget widget) where TWidget : Widget
	{
		var genericWidget = widget as Widget;
		widget.AddEvents((int)(EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.EnterNotifyMask | EventMask.LeaveNotifyMask));
		genericWidget.ObserveEvent(w => w.Events().EnterNotifyEvent).Subscribe(_ => widget.SetStateFlags(StateFlags.Prelight, true));
		genericWidget.ObserveEvent(w => w.Events().LeaveNotifyEvent).Where(_ => !widget.IsPointerInside()).Subscribe(_ => widget.SetStateFlags(StateFlags.Normal, true));
		genericWidget.ObserveEvent(w => w.Events().ButtonPressEvent).Subscribe(_ => widget.SetStateFlags(StateFlags.Active, true));
		genericWidget.ObserveEvent(w => w.Events().ButtonReleaseEvent).Subscribe(_ => widget.SetStateFlags(StateFlags.Prelight, true));
		return widget;
	}

	public static void CenterAbove(this Window window, Widget widget)
	{
		if (!window.Visible) return;

		widget.Window.GetRootCoords(0, 0, out var x, out var y);

		var windowX = x - window.Window.Width / 2 + widget.Window.Width / 2;
		var windowY = y - window.Window.Height - 16;

		window.Move(windowX, windowY);
	}

	public static void CenterOnScreenAtBottom(this Window window, Monitor monitor)
	{
		if (!window.Visible) return;

		Property.Get(window.Display.DefaultScreen.RootWindow, Atom.Intern("_NET_WORKAREA", false), Atom.Intern("CARDINAL", false), 0, 4 * 4, false, out var workArea);
		var fullWorkArea = new Rectangle(0, 0, workArea[2], workArea[3]);
		fullWorkArea.Intersect(monitor.Geometry, out var monitorWorkArea);
		var windowX = monitorWorkArea.Left + monitor.Geometry.Width / 2 - window.Allocation.Width / 2;
		var windowY = monitorWorkArea.Height - window.Allocation.Height - 16;
		window.Move(windowX, windowY);
	}

	public static IObservable<T> TakeUntilDestroyed<T>(this IObservable<T> obs, Widget source)
	{
		return obs.TakeUntil(source.Events().Destroyed.Take(1));
	}

	public static IObservable<T> ObserveEvent<T>(this Widget widget, IObservable<T> obs)
	{
		return obs.TakeUntilDestroyed(widget);
	}

	public static IObservable<T> ObserveEvent<TWidget, T>(this TWidget widget, Func<TWidget, IObservable<T>> f) where TWidget : Widget
	{
		return f(widget).TakeUntilDestroyed(widget);
	}

	public static T AddClass<T>(this T widget, params string[] classes) where T : Widget
	{
		foreach (var c in classes) widget.StyleContext.AddClass(c);
		return widget;
	}

	public static T RemoveClass<T>(this T widget, params string[] classes) where T : Widget
	{
		foreach (var c in classes) widget.StyleContext.RemoveClass(c);
		return widget;
	}

	public static T AddMany<T>(this T widget, params Widget[] children) where T : Container
	{
		foreach (var c in children) widget.Add(c);
		return widget;
	}

	public static void RoundedRectangle(this Context cr, int x, int y, int width, int height, int cornerRadius)
	{
		cr.RoundedRectangle(x, y, width, height, cornerRadius, cornerRadius, cornerRadius, cornerRadius);
	}

	private static void RoundedRectangle(this Context cr, int x, int y, int width, int height, int upperRightRadius, int lowerRightRadius, int lowerLeftRadius, int upperLeftRadius)
	{
		var degrees = Math.PI / 180.0;

		cr.NewSubPath();
		cr.Arc(x + width - upperRightRadius, y + upperRightRadius, upperRightRadius, -90 * degrees, 0 * degrees);
		cr.Arc(x + width - lowerRightRadius, y + height - lowerRightRadius, lowerRightRadius, 0 * degrees, 90 * degrees);
		cr.Arc(x + lowerLeftRadius, y + height - lowerLeftRadius, lowerLeftRadius, 90 * degrees, 180 * degrees);
		cr.Arc(x + upperLeftRadius, y + upperLeftRadius, upperLeftRadius, 180 * degrees, 270 * degrees);
		cr.ClosePath();
	}
}
