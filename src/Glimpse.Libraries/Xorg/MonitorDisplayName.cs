using System.Runtime.InteropServices;
using MentorLake.Gdk;
using MentorLake.GdkX11;

namespace Glimpse.Libraries.Xorg;

public static class MonitorDisplayName
{
	public static string FromGdkMonitor(GdkMonitorHandle monitor, int index)
	{
		var connectorOrModel = monitor.GetModel()?.Trim() ?? "";
		var manufacturerCode = monitor.GetManufacturer()?.Trim() ?? "";
		var productName = "";
		var connector = "";

		if (TryReadEdid(monitor, out var edidManufacturer, out var edidProduct))
		{
			// On X11, gdk_monitor_get_model() is the output/connector name (e.g. DP-0.2).
			connector = connectorOrModel;
			productName = edidProduct;
			if (string.IsNullOrWhiteSpace(manufacturerCode))
			{
				manufacturerCode = edidManufacturer;
			}
		}
		else
		{
			// Other backends may put the product string in model.
			productName = connectorOrModel;
		}

		var manufacturer = PnpIds.Resolve(manufacturerCode);
		var friendly = BuildFriendlyName(manufacturer, productName);
		var name = BuildDisplayName(friendly, connector, index);

		if (monitor.IsPrimary())
		{
			name = $"{name} (Primary)";
		}

		return name;
	}

	private static string BuildFriendlyName(string manufacturer, string productName)
	{
		if (!string.IsNullOrWhiteSpace(manufacturer) && !string.IsNullOrWhiteSpace(productName))
		{
			// Avoid "ASUS ASUS VG258" style duplication.
			if (productName.StartsWith(manufacturer, StringComparison.OrdinalIgnoreCase))
			{
				return productName;
			}

			return $"{manufacturer} {productName}";
		}

		if (!string.IsNullOrWhiteSpace(productName))
		{
			return productName;
		}

		return manufacturer ?? "";
	}

	private static string BuildDisplayName(string friendly, string connector, int index)
	{
		if (!string.IsNullOrWhiteSpace(friendly) && !string.IsNullOrWhiteSpace(connector)
		    && !friendly.Equals(connector, StringComparison.OrdinalIgnoreCase))
		{
			return $"{friendly} ({connector})";
		}

		if (!string.IsNullOrWhiteSpace(friendly))
		{
			return friendly;
		}

		if (!string.IsNullOrWhiteSpace(connector))
		{
			return connector;
		}

		return $"Monitor {index + 1}";
	}

	private static bool TryReadEdid(GdkMonitorHandle monitor, out string manufacturerCode, out string productName)
	{
		manufacturerCode = "";
		productName = "";

		try
		{
			var output = GdkX11MonitorHandle.GetOutput(monitor);
			if (output == 0)
			{
				return false;
			}

			var xDisplayHandle = monitor.GetDisplay().GetXdisplay();
			if (xDisplayHandle.IsInvalid)
			{
				return false;
			}

			var display = (ulong) xDisplayHandle.DangerousGetHandle();
			var edidAtom = XLib.XInternAtom(display, "EDID", true);
			if (edidAtom == 0)
			{
				edidAtom = XLib.XInternAtom(display, "EDID_DATA", true);
			}

			if (edidAtom == 0)
			{
				return false;
			}

			// length is in 32-bit units; 128 reads up to 512 bytes of EDID.
			var status = XRandr.XRRGetOutputProperty(
				display,
				output,
				edidAtom,
				0,
				128,
				false,
				false,
				0,
				out _,
				out var actualFormat,
				out var nitems,
				out _,
				out var prop);

			if (status != 0 || prop == IntPtr.Zero || nitems == 0 || actualFormat != 8)
			{
				if (prop != IntPtr.Zero)
				{
					XLib.XFree(prop);
				}

				return false;
			}

			var bytes = new byte[nitems];
			Marshal.Copy(prop, bytes, 0, (int) nitems);
			XLib.XFree(prop);

			return Edid.TryParse(bytes, out manufacturerCode, out productName);
		}
		catch
		{
			return false;
		}
	}
}
