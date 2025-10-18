using System.Runtime.InteropServices;
using MentorLake.GLib;

namespace Glimpse.Libraries.Xfce.SessionManagement;

internal static class LibXfconfExterns
{
	private const string LibXfconfDll = "libxfconf-0.so.3";

	[DllImport(LibXfconfDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern void xfconf_init(IntPtr ptr);

	[DllImport(LibXfconfDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr xfconf_channel_get(string channelName);

	[DllImport(LibXfconfDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool xfconf_channel_set_string(IntPtr channel, string path, string value);

	[DllImport(LibXfconfDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern bool xfconf_channel_set_uint(IntPtr channel, string path, uint value);

	[DllImport(LibXfconfDll, CallingConvention = CallingConvention.Cdecl)]
	public static extern GHashTableHandle xfconf_channel_get_properties(IntPtr channel, string path);
}
