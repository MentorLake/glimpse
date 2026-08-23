using System.Text;

namespace Glimpse.Libraries.Xorg;

internal static class Edid
{
	public static bool TryParse(ReadOnlySpan<byte> data, out string manufacturerCode, out string productName)
	{
		manufacturerCode = "";
		productName = "";

		if (data.Length < 128)
		{
			return false;
		}

		// Header magic: 00 FF FF FF FF FF FF 00
		if (data[0] != 0x00 || data[1] != 0xFF || data[7] != 0x00)
		{
			return false;
		}

		var id = (data[8] << 8) | data[9];
		manufacturerCode = string.Concat(
			(char) (((id >> 10) & 0x1F) + 64),
			(char) (((id >> 5) & 0x1F) + 64),
			(char) ((id & 0x1F) + 64));

		// Four 18-byte descriptors starting at offset 54. Tag 0xFC is the monitor name.
		for (var i = 0; i < 4; i++)
		{
			var offset = 54 + i * 18;
			if (data[offset] != 0 || data[offset + 1] != 0 || data[offset + 2] != 0 || data[offset + 3] != 0xFC)
			{
				continue;
			}

			var text = data.Slice(offset + 5, 13);
			var length = text.Length;
			for (var j = 0; j < text.Length; j++)
			{
				if (text[j] == 0x0A || text[j] == 0x00)
				{
					length = j;
					break;
				}
			}

			productName = Encoding.ASCII.GetString(text[..length]).Trim();
			break;
		}

		return true;
	}
}
