using System.Runtime.InteropServices;

namespace Glimpse.Common.System.Runtime.InteropServices;

public class MarshalExtensions
{
	public static string[] PtrToStringArray(IntPtr stringArrayPointer)
	{
		if (stringArrayPointer == IntPtr.Zero) return Array.Empty<string>();

		var length = 0;

		while (Marshal.ReadIntPtr(stringArrayPointer, length * IntPtr.Size) != IntPtr.Zero) ++length;
		var stringArray = new string[length];

		for (var index = 0; index < length; ++index)
		{
			var ptr = Marshal.ReadIntPtr(stringArrayPointer, index * IntPtr.Size);
			stringArray[index] = Marshal.PtrToStringAuto(ptr);
		}
		return stringArray;
	}
}
