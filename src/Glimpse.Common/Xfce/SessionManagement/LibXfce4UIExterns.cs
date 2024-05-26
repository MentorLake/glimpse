using System.Runtime.InteropServices;

namespace Glimpse.Common.Xfce.SessionManagement;

internal static class LibXfce4UIExterns
{
	private const string LibXfce4UiDll = "libxfce4ui-2.so.0";

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfce_sm_client_set_restart_style(XfceSMClientHandle client, int restartStyle);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern XfceSMClientHandle xfce_sm_client_get();

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfce_sm_client_set_priority(XfceSMClientHandle client, byte priority);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfce_sm_client_connect(XfceSMClientHandle client, out IntPtr error);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfce_sm_client_set_restart_command(XfceSMClientHandle client, string[] command);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfce_sm_client_set_current_directory(XfceSMClientHandle client, string directory);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NoNativeFreeStringMarshaller))]
	public static extern string xfce_sm_client_get_client_id(XfceSMClientHandle client);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfce_sm_client_disconnect(XfceSMClientHandle client);

	[DllImport(LibXfce4UiDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern XfceSMClientHandle xfce_sm_client_get_with_argv(int numCommandLineArgs, string[] commandLineArgs, int restartStyle, byte priority);

	public enum RestartStyle
	{
		IfRunning = 0,
		Immediately = 1,
	}
}

public sealed class NoNativeFreeStringMarshaller : ICustomMarshaler
{
	private static readonly NoNativeFreeStringMarshaller s_instance = new();

	public void CleanUpManagedData(object o)
	{

	}

	public void CleanUpNativeData(IntPtr ptr)
	{

	}

	public int GetNativeDataSize()
	{
		return IntPtr.Size;
	}

	public IntPtr MarshalManagedToNative(object o)
	{
		return Marshal.StringToHGlobalAuto((string)o);
	}

	public object MarshalNativeToManaged(IntPtr ptr)
	{
		return Marshal.PtrToStringAuto(ptr);
	}

	public static ICustomMarshaler GetInstance(string s)
	{
		return s_instance;
	}
}
