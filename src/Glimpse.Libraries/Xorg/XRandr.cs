using System.Runtime.InteropServices;

namespace Glimpse.Libraries.Xorg;

internal static class XRandr
{
	private const string LibraryName = "libXrandr.so.2";

	[DllImport(LibraryName)]
	public static extern int XRRGetOutputProperty(
		ulong dpy,
		ulong output,
		ulong property,
		long offset,
		long length,
		bool delete,
		bool pending,
		ulong reqType,
		out ulong actualType,
		out int actualFormat,
		out ulong nitems,
		out ulong bytesAfter,
		out IntPtr prop);
}
