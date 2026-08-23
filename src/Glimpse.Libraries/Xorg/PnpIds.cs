namespace Glimpse.Libraries.Xorg;

internal static class PnpIds
{
	private static readonly Lazy<Dictionary<string, string>> Ids = new(Load);

	public static string Resolve(string manufacturerCode)
	{
		if (string.IsNullOrWhiteSpace(manufacturerCode))
		{
			return "";
		}

		var code = manufacturerCode.Trim().ToUpperInvariant();
		return Ids.Value.TryGetValue(code, out var name) ? name : "";
	}

	private static Dictionary<string, string> Load()
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var paths = new[]
		{
			"/usr/share/hwdata/pnp.ids",
			"/usr/share/misc/pnp.ids",
			"/usr/share/libgnome-desktop-3.0/pnp.ids"
		};

		foreach (var path in paths)
		{
			if (!File.Exists(path))
			{
				continue;
			}

			foreach (var line in File.ReadLines(path))
			{
				if (line.Length < 4 || line[0] == '#' || char.IsWhiteSpace(line[0]))
				{
					continue;
				}

				// Format: "VSCViewSonic Corporation" (3-char code, no separator)
				var code = line[..3];
				var name = line[3..].Trim();
				if (name.Length > 0)
				{
					result.TryAdd(code, name);
				}
			}

			break;
		}

		return result;
	}
}
