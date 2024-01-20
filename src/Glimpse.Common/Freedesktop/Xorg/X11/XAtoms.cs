using Glimpse.Interop.XLib;

namespace Glimpse.Xorg.X11;

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
	public static readonly ulong NetWmMoveresize;
	public static readonly ulong WmClass;
	public static readonly ulong NetWmStateDemandsAttention;
	public static readonly ulong WmProtocols;
	public static readonly ulong WmDeleteWindow;
	public static readonly ulong NetWmStateSkipTaskbar;

	static XAtoms()
	{
		var display = XLib.XOpenDisplay(0);

		String = XLib.XInternAtom(display, "STRING", true);
		Utf8String = XLib.XInternAtom(display, "UTF8_STRING", true);
		CompoundText = XLib.XInternAtom(display, "COMPOUND_TEXT", true);
		WmName = XLib.XInternAtom(display, "WM_NAME", true);
		NetWmState = XLib.XInternAtom(display, "_NET_WM_STATE", true);
		NetWmName = XLib.XInternAtom(display, "_NET_WM_NAME", true);
		NetWmIconName = XLib.XInternAtom(display, "_NET_WM_ICON_NAME", true);
		NetWmIcon = XLib.XInternAtom(display, "_NET_WM_ICON", true);
		NetWmPid = XLib.XInternAtom(display, "_NET_WM_PID", true);
		NetWmWindowType = XLib.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true);
		NetClientList = XLib.XInternAtom(display, "_NET_CLIENT_LIST", true);
		WinClientList = XLib.XInternAtom(display, "_WIN_CLIENT_LIST", true);
		NetWmWindowTypeNormal = XLib.XInternAtom(display, "_NET_WM_WINDOW_TYPE_NORMAL", true);
		WmChangeState = XLib.XInternAtom(display, "WM_CHANGE_STATE", true);
		NetWmAllowedActions = XLib.XInternAtom(display, "_NET_WM_ALLOWED_ACTIONS", true);
		NetCloseWindow = XLib.XInternAtom(display, "_NET_CLOSE_WINDOW", true);
		NetActiveWindow = XLib.XInternAtom(display, "_NET_ACTIVE_WINDOW", true);
		NetWmStateMaximizedVert = XLib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_VERT", true);
		NetWmStateMaximizedHorz = XLib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_HORZ", true);
		NetWmStateHidden = XLib.XInternAtom(display, "_NET_WM_STATE_HIDDEN", true);
		NetWmMoveresize = XLib.XInternAtom(display, "_NET_WM_MOVERESIZE", true);
		WmClass = XLib.XInternAtom(display, "WM_CLASS", true);
		NetWmStateDemandsAttention = XLib.XInternAtom(display, "_NET_WM_STATE_DEMANDS_ATTENTION", true);
		WmProtocols = XLib.XInternAtom(display, "WM_PROTOCOLS", true);
		WmDeleteWindow = XLib.XInternAtom(display, "WM_DELETE_WINDOW", true);
		NetWmStateSkipTaskbar = XLib.XInternAtom(display, "_NET_WM_STATE_SKIP_TASKBAR", true);

		XLib.XCloseDisplay(display);
	}
}
