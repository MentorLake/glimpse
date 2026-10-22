namespace Glimpse.Libraries.Xorg.X11;

public class XAtom
{
	public XAtom(ulong atomId, string name)
	{

	}
}

internal static class XAtoms
{
	public static readonly ulong String;
	public static readonly ulong Utf8String;
	public static readonly ulong CompoundText;
	public static readonly ulong Targets;
	public static readonly ulong Timestamp;
	public static readonly ulong Multiple;
	public static readonly ulong Clipboard;
	public static readonly ulong Atom;
	public static readonly ulong Integer;
	public static readonly ulong WmName;
	public static readonly ulong NetWmState;
	public static readonly ulong NetWmName;
	public static readonly ulong NetWmIconName;
	public static readonly ulong NetWmIcon;
	public static readonly ulong NetWmPid;
	public static readonly ulong NetWmWindowType;
	public static readonly ulong NetClientList;
	public static readonly ulong WinClientList;
	public static readonly ulong NetWmWindowTypeNormal;
	public static readonly ulong WmChangeState;
	public static readonly ulong NetWmAllowedActions;
	public static readonly ulong NetCloseWindow;
	public static readonly ulong NetActiveWindow;
	public static readonly ulong NetWmStateMaximizedVert;
	public static readonly ulong NetWmStateMaximizedHorz;
	public static readonly ulong NetWmStateHidden;
	public static readonly ulong NetWmStateFullscreen;
	public static readonly ulong NetWmMoveresize;
	public static readonly ulong NetMoveresizeWindow;
	public static readonly ulong WmClass;
	public static readonly ulong NetWmStateDemandsAttention;
	public static readonly ulong WmProtocols;
	public static readonly ulong WmDeleteWindow;
	public static readonly ulong NetWmStateSkipTaskbar;

	static XAtoms()
	{
		var display = XLib.XOpenDisplay(0);

		String = XLib.XInternAtom(display, "STRING", false);
		Utf8String = XLib.XInternAtom(display, "UTF8_STRING", false);
		CompoundText = XLib.XInternAtom(display, "COMPOUND_TEXT", false);
		Targets = XLib.XInternAtom(display, "TARGETS", false);
		Timestamp = XLib.XInternAtom(display, "TIMESTAMP", false);
		Multiple = XLib.XInternAtom(display, "MULTIPLE", false);
		Clipboard = XLib.XInternAtom(display, "CLIPBOARD", false);
		Atom = XLib.XInternAtom(display, "ATOM", false);
		Integer = XLib.XInternAtom(display, "INTEGER", false);
		WmName = XLib.XInternAtom(display, "WM_NAME", false);
		NetWmState = XLib.XInternAtom(display, "_NET_WM_STATE", false);
		NetWmName = XLib.XInternAtom(display, "_NET_WM_NAME", false);
		NetWmIconName = XLib.XInternAtom(display, "_NET_WM_ICON_NAME", false);
		NetWmIcon = XLib.XInternAtom(display, "_NET_WM_ICON", false);
		NetWmPid = XLib.XInternAtom(display, "_NET_WM_PID", false);
		NetWmWindowType = XLib.XInternAtom(display, "_NET_WM_WINDOW_TYPE", false);
		NetClientList = XLib.XInternAtom(display, "_NET_CLIENT_LIST", false);
		WinClientList = XLib.XInternAtom(display, "_WIN_CLIENT_LIST", false);
		NetWmWindowTypeNormal = XLib.XInternAtom(display, "_NET_WM_WINDOW_TYPE_NORMAL", false);
		WmChangeState = XLib.XInternAtom(display, "WM_CHANGE_STATE", false);
		NetWmAllowedActions = XLib.XInternAtom(display, "_NET_WM_ALLOWED_ACTIONS", false);
		NetCloseWindow = XLib.XInternAtom(display, "_NET_CLOSE_WINDOW", false);
		NetActiveWindow = XLib.XInternAtom(display, "_NET_ACTIVE_WINDOW", false);
		NetWmStateMaximizedVert = XLib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_VERT", false);
		NetWmStateMaximizedHorz = XLib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
		NetWmStateHidden = XLib.XInternAtom(display, "_NET_WM_STATE_HIDDEN", false);
		NetWmStateFullscreen = XLib.XInternAtom(display, "_NET_WM_STATE_FULLSCREEN", false);
		NetWmMoveresize = XLib.XInternAtom(display, "_NET_WM_MOVERESIZE", false);
		NetMoveresizeWindow = XLib.XInternAtom(display, "_NET_MOVERESIZE_WINDOW", false);
		WmClass = XLib.XInternAtom(display, "WM_CLASS", false);
		NetWmStateDemandsAttention = XLib.XInternAtom(display, "_NET_WM_STATE_DEMANDS_ATTENTION", false);
		WmProtocols = XLib.XInternAtom(display, "WM_PROTOCOLS", false);
		WmDeleteWindow = XLib.XInternAtom(display, "WM_DELETE_WINDOW", false);
		NetWmStateSkipTaskbar = XLib.XInternAtom(display, "_NET_WM_STATE_SKIP_TASKBAR", false);

		XLib.XCloseDisplay(display);
	}
}

public class XAtomsInstance
{
	public readonly ulong String;
	public readonly ulong Utf8String;
	public readonly ulong CompoundText;
	public readonly ulong Targets;
	public readonly ulong Timestamp;
	public readonly ulong Multiple;
	public readonly ulong Clipboard;
	public readonly ulong Atom;
	public readonly ulong Integer;
	public readonly ulong WmName;
	public readonly ulong NetWmState;
	public readonly ulong NetWmName;
	public readonly ulong NetWmIconName;
	public readonly ulong NetWmIcon;
	public readonly ulong NetWmPid;
	public readonly ulong NetWmWindowType;
	public readonly ulong NetClientList;
	public readonly ulong WinClientList;
	public readonly ulong NetWmWindowTypeNormal;
	public readonly ulong WmChangeState;
	public readonly ulong NetWmAllowedActions;
	public readonly ulong NetCloseWindow;
	public readonly ulong NetActiveWindow;
	public readonly ulong NetWmStateMaximizedVert;
	public readonly ulong NetWmStateMaximizedHorz;
	public readonly ulong NetWmStateHidden;
	public readonly ulong NetWmStateFullscreen;
	public readonly ulong NetWmMoveresize;
	public readonly ulong NetMoveresizeWindow;
	public readonly ulong WmClass;
	public readonly ulong NetWmStateDemandsAttention;
	public readonly ulong WmProtocols;
	public readonly ulong WmDeleteWindow;
	public readonly ulong NetWmStateSkipTaskbar;

	public XAtomsInstance(ulong display)
	{
		String = XLib.XInternAtom(display, "STRING", false);
		Utf8String = XLib.XInternAtom(display, "UTF8_STRING", false);
		CompoundText = XLib.XInternAtom(display, "COMPOUND_TEXT", false);
		Targets = XLib.XInternAtom(display, "TARGETS", false);
		SaveTargets = XLib.XInternAtom(display, "SAVE_TARGETS", false);
		Timestamp = XLib.XInternAtom(display, "TIMESTAMP", false);
		Multiple = XLib.XInternAtom(display, "MULTIPLE", false);
		Clipboard = XLib.XInternAtom(display, "CLIPBOARD", false);
		Primary = XLib.XInternAtom(display, "PRIMARY", false);
		ClipboardManager = XLib.XInternAtom(display, "CLIPBOARD_MANAGER", false);
		Incr = XLib.XInternAtom(display, "INCR", false);
		None = XLib.XInternAtom(display, "NONE", false);
		Text = XLib.XInternAtom(display, "TEXT", false);
		Atom = XLib.XInternAtom(display, "ATOM", false);
		Integer = XLib.XInternAtom(display, "INTEGER", false);
		WmName = XLib.XInternAtom(display, "WM_NAME", false);
		NetWmState = XLib.XInternAtom(display, "_NET_WM_STATE", false);
		NetWmName = XLib.XInternAtom(display, "_NET_WM_NAME", false);
		NetWmIconName = XLib.XInternAtom(display, "_NET_WM_ICON_NAME", false);
		NetWmIcon = XLib.XInternAtom(display, "_NET_WM_ICON", false);
		NetWmPid = XLib.XInternAtom(display, "_NET_WM_PID", false);
		NetWmWindowType = XLib.XInternAtom(display, "_NET_WM_WINDOW_TYPE", false);
		NetClientList = XLib.XInternAtom(display, "_NET_CLIENT_LIST", false);
		WinClientList = XLib.XInternAtom(display, "_WIN_CLIENT_LIST", false);
		NetWmWindowTypeNormal = XLib.XInternAtom(display, "_NET_WM_WINDOW_TYPE_NORMAL", false);
		WmChangeState = XLib.XInternAtom(display, "WM_CHANGE_STATE", false);
		NetWmAllowedActions = XLib.XInternAtom(display, "_NET_WM_ALLOWED_ACTIONS", false);
		NetCloseWindow = XLib.XInternAtom(display, "_NET_CLOSE_WINDOW", false);
		NetActiveWindow = XLib.XInternAtom(display, "_NET_ACTIVE_WINDOW", false);
		NetWmStateMaximizedVert = XLib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_VERT", false);
		NetWmStateMaximizedHorz = XLib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
		NetWmStateHidden = XLib.XInternAtom(display, "_NET_WM_STATE_HIDDEN", false);
		NetWmStateFullscreen = XLib.XInternAtom(display, "_NET_WM_STATE_FULLSCREEN", false);
		NetWmMoveresize = XLib.XInternAtom(display, "_NET_WM_MOVERESIZE", false);
		NetMoveresizeWindow = XLib.XInternAtom(display, "_NET_MOVERESIZE_WINDOW", false);
		WmClass = XLib.XInternAtom(display, "WM_CLASS", false);
		NetWmStateDemandsAttention = XLib.XInternAtom(display, "_NET_WM_STATE_DEMANDS_ATTENTION", false);
		WmProtocols = XLib.XInternAtom(display, "WM_PROTOCOLS", false);
		WmDeleteWindow = XLib.XInternAtom(display, "WM_DELETE_WINDOW", false);
		NetWmStateSkipTaskbar = XLib.XInternAtom(display, "_NET_WM_STATE_SKIP_TASKBAR", false);
	}

	public ulong SaveTargets { get; set; }

	public ulong Text { get; set; }

	public ulong Primary { get; set; }

	public ulong ClipboardManager { get; set; }

	public ulong Incr { get; set; }

	public ulong None { get; set; }
}
