using System.Buffers;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.Xorg.X11;

internal static unsafe class X11Extensions
{
	public static IObservable<XPropertyEvent> ObservePropertyEvent(this IObservable<(XAnyEvent someEvent, IntPtr eventPointer)> obs)
	{
		return obs
			.Where(t => t.someEvent.type == (int)Event.PropertyNotify)
			.Select(t => Marshal.PtrToStructure<XPropertyEvent>(t.eventPointer));
	}

	public static IObservable<ImmutableList<GdkPixbufHandle>> ObserveIcons(this IObservable<XPropertyEvent> obs, ulong property)
	{
		return obs
			.Where(e => e.atom == property)
			.Select(e => new XWindowRef() { Window = e.window, Display = e.display }.GetIcons())
			.DistinctUntilChanged();
	}

	public static IObservable<ulong[]> ObserveAtomArray(this IObservable<XPropertyEvent> obs, ulong property)
	{
		return obs
			.Where(e => e.atom == property)
			.Select(e => GetAtomArray(new XWindowRef() { Window = e.window, Display = e.display }, property))
			.DistinctUntilChanged();
	}

	public static IObservable<List<string>> ObserveAtomNameArray(this IObservable<XPropertyEvent> obs, ulong property)
	{
		return obs
			.Where(e => e.atom == property)
			.Select(e => GetAtomNameArray(new XWindowRef() { Window = e.window, Display = e.display }, property))
			.DistinctUntilChanged()
			.Select(l => l.ToList());
	}

	public static IObservable<string> ObserveStringProperty(this IObservable<XPropertyEvent> obs, ulong property)
	{
		return obs
			.Where(e => e.atom == property)
			.Select(e => GetStringProperty(new XWindowRef() { Window = e.window, Display = e.display }, property))
			.DistinctUntilChanged();
	}

	public static IObservable<ulong[]> ObserveULongArrayProperty(this IObservable<XPropertyEvent> obs, ulong property)
	{
		return obs
			.Where(e => e.atom == property)
			.Select(e => GetULongArray(new XWindowRef() { Window = e.window, Display = e.display }, property))
			.DistinctUntilChanged();
	}

	public static string GetStringProperty(this XWindowRef windowRef, ulong property)
	{
		var result = XLib.XGetWindowProperty(windowRef.Display, windowRef.Window, property, 0, 1024, false, 0, out var actualTypeReturn, out var actualFormatReturn, out var actualLength, out _, out var dataPointer);

		if (result != 0 || actualTypeReturn == 0)
		{
			return null;
		}

		var atomName = XLib.XGetAtomName(windowRef.Display, actualTypeReturn);

		if (atomName == "STRING" || atomName == "UTF8_STRING" || atomName == "COMPOUND_TEXT")
		{
			var dataSize = (int)(actualLength * (ulong)actualFormatReturn / 8);
			var propBytes = new byte[dataSize];
			Marshal.Copy(dataPointer, propBytes, 0, dataSize);
			XLib.XFree(dataPointer);
			return Encoding.UTF8.GetString(propBytes);
		}

		return null;
	}

	public static ulong[] GetULongArray(this XWindowRef windowRef, ulong property)
	{
		var result = XLib.XGetWindowProperty(windowRef.Display, windowRef.Window, property, 0, 1024, false, 0, out var actualTypeReturn, out var actualFormatReturn, out var actualLength, out _, out var dataPointer);
		if (result != 0) return Array.Empty<ulong>();
		var dataSize = (int)(actualLength * sizeof(ulong));
		if (dataSize == 0) return Array.Empty<ulong>();

		var data = new byte[dataSize];
		Marshal.Copy(dataPointer, data, 0, dataSize);
		XLib.XFree(dataPointer);
		using var reader = new BinaryReader(new MemoryStream(data));
		var atomNames = new LinkedList<ulong>();

		for (var i = 0; i < (int) actualLength; i++)
		{
			atomNames.AddLast((ulong) reader.ReadInt64());
		}

		return atomNames.ToArray();
	}

	public static ulong[] GetAtomArray(this XWindowRef windowRef, ulong property)
	{
		var result = XLib.XGetWindowProperty(windowRef.Display, windowRef.Window, property, 0, 1024, false, 0, out var actualTypeReturn, out var actualFormatReturn, out var actualLength, out _, out var dataPointer);
		if (result != 0) return Array.Empty<ulong>();
		var dataSize = (int)(actualLength * (ulong)actualFormatReturn / 8);
		if (dataSize == 0) return Array.Empty<ulong>();

		var data = new byte[dataSize];
		Marshal.Copy(dataPointer, data, 0, dataSize);
		XLib.XFree(dataPointer);
		using var reader = new BinaryReader(new MemoryStream(data));
		var atoms = new LinkedList<ulong>();

		for (var i = 0; i < (int) actualLength; i++)
		{
			var atom = (ulong)reader.ReadInt32();
			if (atom == 0) continue;
			atoms.AddLast(atom);
		}

		return atoms.ToArray();
	}

	public static string[] GetAtomNameArray(this XWindowRef windowRef, ulong property)
	{
		var atoms = windowRef.GetAtomArray(property);
		var atomNames = new LinkedList<string>();

		for (var i = 0; i < atoms.Length; i++)
		{
			atomNames.AddLast(XLib.XGetAtomName(windowRef.Display, atoms[i]));
		}

		return atomNames.ToArray();
	}

	public static bool IsNormalWindow(this XWindowRef windowRef)
	{
		var windowType = windowRef.GetAtomNameArray(XAtoms.NetWmWindowType);

		if (windowType.Contains("_NET_WM_WINDOW_TYPE_NORMAL") || windowType.Contains("_NET_WM_WINDOW_TYPE_DIALOG") || !windowType.Any())
		{
			var state = windowRef.GetAtomArray(XAtoms.NetWmState);
			return !state.Contains(XAtoms.NetWmStateSkipTaskbar);
		}

		return false;
	}

	public static ImmutableList<GdkPixbufHandle> GetIcons(this XWindowRef windowRef)
	{
		var success = XLib.XGetWindowProperty(windowRef.Display, windowRef.Window, XAtoms.NetWmIcon, 0, 1024 * 1024 * 10, false, 0, out var actualTypeReturn, out _, out var actualLength, out _, out var dataPointer);

		if (success != 0 || actualTypeReturn == 0) return ImmutableList<GdkPixbufHandle>.Empty;

		using var memoryStream = new UnmanagedMemoryStream((byte*)dataPointer, (long)actualLength * 8);
		using var binaryReader = new BinaryReader(memoryStream);
		var icons = ImmutableList<GdkPixbufHandle>.Empty;

		while (binaryReader.PeekChar() != -1)
		{
			var width = binaryReader.ReadInt64();
			var height = binaryReader.ReadInt64();
			var numPixels = width * height;
			var imageData = ArrayPool<byte>.Shared.Rent((int) numPixels * sizeof(int));

			for (var i = 0; i < numPixels * sizeof(int); i += sizeof(int))
			{
				imageData[i] = binaryReader.ReadByte();
				imageData[i+1] = binaryReader.ReadByte();
				imageData[i+2] = binaryReader.ReadByte();
				imageData[i+3] = binaryReader.ReadByte();
				binaryReader.ReadInt32();
			}

			icons = icons.Add(GdkPixbufFactory.From(imageData, 32, (int) width, (int) height));
			ArrayPool<byte>.Shared.Return(imageData);
		}

		XLib.XFree(dataPointer);

		return icons;
	}
}
