using System.Runtime.InteropServices;

namespace Glimpse.Common.Xorg;

public class XLib
{
	private const string LibraryName = "libX11.so.6";

	[DllImport(LibraryName)]
	public static extern ulong XOpenDisplay(ulong display);

	[DllImport(LibraryName)]
	public static extern int XCloseDisplay(ulong display);

	[DllImport(LibraryName)]
	public static extern int XSelectInput(ulong display, ulong window, EventMask event_mask);

	[DllImport(LibraryName)]
	public static extern int XNextEvent(ulong display, IntPtr e);

	[DllImport(LibraryName)]
	public static extern ulong XDefaultRootWindow(ulong display);

	[DllImport(LibraryName)]
	public static extern string XGetAtomName(ulong display, ulong atom);

	[DllImport(LibraryName)]
	public static extern void XFree(IntPtr data);

	[DllImport(LibraryName)]
	public static extern int XInitThreads();

	[DllImport(LibraryName, EntryPoint = "XQueryTree")]
	public static extern int XQueryTree(
		ulong display, // Display* display
		ulong window, // Window window
		out ulong root_return,
		out ulong parent_return,
		out IntPtr children_return, // Window* children_return
		out uint nchildren_return);

	[DllImport(LibraryName)]
	public static extern ulong XInternAtom(ulong display, string atom_name, bool only_if_exists);

	[DllImport(LibraryName)]
	public static extern int XGetWindowProperty(
		ulong display,
		ulong window,
		ulong property,
		long offset,
		long length,
		bool delete,
		ulong requestType,
		out ulong actualTypeReturn,
		out int actualFormatReturn,
		out ulong numItemsReturned,
		out ulong bytesLeft,
		out IntPtr data);

	[DllImport(LibraryName)]
	public static extern int XSetInputFocus(ulong display, ulong window, int revertTo, ulong time);

	[DllImport(LibraryName)]
	public static extern int XRaiseWindow(ulong display, ulong window);

	[DllImport(LibraryName)]
	public static extern int XLowerWindow(ulong display, ulong window);

	[DllImport(LibraryName)]
	public static extern int XGetInputFocus(ulong display, out ulong focus_return, out int revert_to_return);

	[DllImport(LibraryName)]
	public static extern int XMapWindow(ulong display, ulong window);

	[DllImport(LibraryName)]
	public static extern int XMapRaised(ulong display, ulong window);

	[DllImport(LibraryName)]
	public static extern int XUnmapWindow(ulong display, ulong window);

	[DllImport(LibraryName)]
	public static extern int XFlush(ulong display);

	[DllImport(LibraryName)]
	public static extern int XIconifyWindow(ulong display, ulong window, int screen_number);

	[DllImport(LibraryName)]
	public static extern int XDefaultScreenOfDisplay(ulong display);

	[DllImport(LibraryName)]
	public static extern int XGetClassHint(ulong display, ulong window, out XClassHint class_hints);

	[DllImport(LibraryName)]
	public static extern int XGetWindowAttributes(ulong display, ulong window, out XWindowAttributes window_attributes);

	[DllImport(LibraryName)]
	public static extern IntPtr XGetImage(ulong display, ulong window, int x, int y, uint width, uint height, ulong plane_mask, int format);

	[DllImport(LibraryName)]
	public static extern int XDestroyImage(IntPtr image);

	[DllImport(LibraryName)]
	public static extern int XSendEvent(ulong display, ulong window, bool propagate, long event_mask, IntPtr send_event);

	[DllImport(LibraryName)]
	public static extern int XWarpPointer(ulong display, ulong src_w, ulong dest_w, int src_x, int src_y, int src_width, int src_height, int dest_x, int dest_y);

	[DllImport(LibraryName)]
	public static extern bool XTranslateCoordinates(ulong display, ulong src_w, ulong dest_w, int src_x, int src_y, out int dest_x_return, out int dest_y_return, out ulong child_return);

	[DllImport(LibraryName)]
	public static extern int XGrabKey(ulong display, byte keycode, uint modifiers, ulong grab_window, bool owner_events, int pointer_mode, int keyboard_mode);

	[DllImport(LibraryName)]
	public static extern int XUngrabKeyboard(ulong display, ulong time);

	[DllImport(LibraryName)]
	public static extern IntPtr XSetErrorHandler(XErrorHandlerDelegate del);

	[DllImport(LibraryName)]
	public static extern IntPtr XSetIOErrorHandler(XIOErrorHandlerDelegate del);

	[DllImport(LibraryName)]
	public static extern void XSetIOErrorExitHandler(ulong display, XIOErrorHandlerDelegate handler, IntPtr userData);

	[DllImport(LibraryName)]
	public static extern int XGetErrorText(IntPtr display, int code, IntPtr description, int length);

	[DllImport(LibraryName)]
	public static extern int XGetErrorDatabaseText(IntPtr display, string name, string message, string default_string, IntPtr buffer_return, int length);

	[DllImport("libXRes.so.1")]
	public static extern int XResQueryClientIds(ulong display, long num_specse, XResClientIdSpec[] client_specs, out long num_ids, out XResClientIdValue[] client_ids);

	[DllImport("libXRes.so.1")]
	public static extern uint XResGetClientPid(ref XResClientIdValue value);
}

public static class XConstants
{
	public const uint AllPlanes = 0xFFFFFFFF;
	public const int ZPixmap = 2; // This value might vary, please confirm the correct value for your system
	public const int KeyCode_Super_L = 133;
	public const int AllModifiers = 1 << 15;

	public const int IsViewable = 2;
}

[StructLayout(LayoutKind.Sequential)]
public struct XImage
{
	public int width, height;
	public int xoffset;
	public int format;
	public IntPtr data;
	public int byte_order;
	public int bitmap_unit;
	public int bitmap_bit_order;
	public int bitmap_pad;
	public int depth;
	public int bytes_per_line;
	public int bits_per_pixel;
	public ulong red_mask;
	public ulong green_mask;
	public ulong blue_mask;
	public IntPtr obdata;
}

[StructLayout(LayoutKind.Sequential)]
public struct XWindowAttributes
{
	public int x, y;
	public uint width, height;
	public int border_width;
	public int depth;
	public IntPtr visual;
	public ulong root;
	public int @class;
	public int bit_gravity;
	public int win_gravity;
	public int backing_store;
	public ulong backing_planes;
	public ulong backing_pixel;
	public bool save_under;
	public ulong colormap;
	public bool map_installed;
	public int map_state;
	public long all_event_masks;
	public long your_event_masks;
	public long do_not_propagate_mask;
	public bool override_redirect;
	public IntPtr screen;
}

public struct XClassHint
{
	public string res_name;
	public string res_class;
}

[Flags]
public enum EventMask : long
{
	NoEventMask = 0L,
	KeyPressMask = (1L << 0),
	KeyReleaseMask = (1L << 1),
	ButtonPressMask = (1L << 2),
	ButtonReleaseMask = (1L << 3),
	EnterWindowMask = (1L << 4),
	LeaveWindowMask = (1L << 5),
	PointerMotionMask = (1L << 6),
	PointerMotionHintMask = (1L << 7),
	Button1MotionMask = (1L << 8),
	Button2MotionMask = (1L << 9),
	Button3MotionMask = (1L << 10),
	Button4MotionMask = (1L << 11),
	Button5MotionMask = (1L << 12),
	ButtonMotionMask = (1L << 13),
	KeymapStateMask = (1L << 14),
	ExposureMask = (1L << 15),
	VisibilityChangeMask = (1L << 16),
	StructureNotifyMask = (1L << 17),
	ResizeRedirectMask = (1L << 18),
	SubstructureNotifyMask = (1L << 19),
	SubstructureRedirectMask = (1L << 20),
	FocusChangeMask = (1L << 21),
	PropertyChangeMask = (1L << 22),
	ColormapChangeMask = (1L << 23),
	OwnerGrabButtonMask = (1L << 24),
}

public enum Event : int
{
	KeyPress = 2,
	KeyRelease = 3,
	ButtonPress = 4,
	ButtonRelease = 5,
	MotionNotify = 6,
	EnterNotify = 7,
	LeaveNotify = 8,
	FocusIn = 9,
	FocusOut = 10,
	KeymapNotify = 11,
	Expose = 12,
	GraphicsExpose = 13,
	NoExpose = 14,
	VisibilityNotify = 15,
	CreateNotify = 16,
	DestroyNotify = 17,
	UnmapNotify = 18,
	MapNotify = 19,
	MapRequest = 20,
	ReparentNotify = 21,
	ConfigureNotify = 22,
	ConfigureRequest = 23,
	GravityNotify = 24,
	ResizeRequest = 25,
	CirculateNotify = 26,
	CirculateRequest = 27,
	PropertyNotify = 28,
	SelectionClear = 29,
	SelectionRequest = 30,
	SelectionNotify = 31,
	ColormapNotify = 32,
	ClientMessage = 33,
	MappingNotify = 34,
	GenericEvent = 35,
	LASTEvent = 36
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XAnyEvent
{
	public int type;
	public ulong serial;
	public bool send_event;
	public ulong display;
	public ulong window;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XKeyEvent
{
	public int type; /* of event */
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public ulong window; /* "event" window it is reported relative to */
	public ulong root; /* root window that the event occurred on */
	public ulong subwindow; /* child window */
	public ulong time; /* milliseconds */
	public int x, y; /* pointer x, y coordinates in event window */
	public int x_root, y_root; /* coordinates relative to root */
	public uint state; /* key or button mask */
	public uint keycode; /* detail */
	public bool same_screen;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XFocusChangeEvent
{
	public int type; /* FocusIn or FocusOut */
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public ulong window; /* window of event */
	NotifyMode mode;
	NotifyDetail detail;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XExposeEvent
{
	public int type;
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public ulong window;
	public int x, y;
	public int width, height;
	public int count;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XVisibilityEvent
{
	public int type;
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public ulong window;
	public int state;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XCreateWindowEvent
{
	public int type;
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public ulong parent; /* parent of the window */
	public ulong window; /* window id of window created */
	public int x, y; /* window location */
	public int width, height; /* size of window */
	public int border_width; /* border width */
	public bool override_redirect;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XPropertyEvent
{
	public int type;
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public ulong window;
	public ulong atom;
	public long time;
	public int state;
}

[StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
public struct XDestroyWindowEvent
{
	public int type;
	public ulong serial; /* # of last request processed by server */
	public bool send_event; /* true if this came from a SendEvent request */
	public ulong display; /* Display the event was read from */
	public IntPtr @event;
	public ulong window;
}

public enum NotifyMode : int
{
	NotifyNormal = 0,
	NotifyGrab = 1,
	NotifyUngrab = 2,
	NotifyWhileGrabbed = 3,
}

public enum NotifyDetail : int
{
	NotifyAncestor = 0,
	NotifyVirtual = 1,
	NotifyInferior = 2,
	NotifyNonlinear = 3,
	NotifyNonlinearVirtual = 4,
	NotifyPointer = 5,
	NotifyPointerRoot = 6,
	NotifyDetailNone = 7,
}

[StructLayout(LayoutKind.Sequential, Size = 24 * sizeof(long))]
public struct XClientMessageEvent
{
	public int type;
	public ulong serial;
	public int send_event;
	public ulong display;
	public ulong window;
	public ulong message_type;
	public int format;
	public IntPtr ptr1;
	public IntPtr ptr2;
	public IntPtr ptr3;
	public IntPtr ptr4;
	public IntPtr ptr5;
}

[StructLayout(LayoutKind.Sequential)]
public struct XErrorEvent
{
	public int type;
	public ulong display;
	public ulong resourceid;
	public ulong serial;
	public byte error_code;
	public byte request_code;
	public byte minor_code;
}

public delegate int XErrorHandlerDelegate(IntPtr display, ref XErrorEvent ev);

public delegate int XIOErrorHandlerDelegate(IntPtr display);

[StructLayout(LayoutKind.Sequential)]
public struct XResClientIdSpec
{
	public ulong client;
	public uint mask;
}

[StructLayout(LayoutKind.Sequential, Size = 1024)]
public struct XResClientIdValue
{
	public XResClientIdSpec spec;
	public long length;
	public IntPtr Values;
	// followed by length CARD32s
}

[StructLayout(LayoutKind.Sequential, Size = 12 + (8 * 24))]
public struct XResQueryClientIds
{
	public byte reqType;
	public byte XResReqType;
	public ushort length;
	public int numSpecs;
	// followed by numSpecs times XResClientIdSpec
}

[StructLayout(LayoutKind.Sequential, Size = 32 + (8 * 24))]
public struct XResQueryClientIdsReply
{
	public byte type;
	public byte pad1;
	public ushort sequenceNumber;
	public int length;
	public int numIds;
	public int pad2;
	public int pad3;
	public int pad4;
	public int pad5;
	public int pad6;
// followed by numIds times XResClientIdValue
}
