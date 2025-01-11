using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MentorLake.GdkPixbuf;

namespace Glimpse.Libraries.Images;

public class GdkPixbufHandleJsonConverter : JsonConverter<GdkPixbufHandle>
{
	public override GdkPixbufHandle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var imageJson = JsonNode.Parse(ref reader, new JsonNodeOptions() { PropertyNameCaseInsensitive = true }) as JsonObject;
		imageJson.TryGetPropertyValue("Data", out var dataNode);

		if (string.IsNullOrEmpty(dataNode.GetValue<string>()))
		{
			return null;
		}

		return GdkPixbufFactory.From(Convert.FromBase64String(dataNode.GetValue<string>()));
	}

	public override void Write(Utf8JsonWriter writer, GdkPixbufHandle pixbuf, JsonSerializerOptions options)
	{
		pixbuf.SaveToBuffer(out var buffer, out _, "png", IntPtr.Zero, IntPtr.Zero);
		writer.WriteStartObject();
		writer.WriteBase64String("Data", buffer);
		writer.WriteNumber("Width", pixbuf.GetWidth());
		writer.WriteNumber("Height", pixbuf.GetHeight());
		writer.WriteEndObject();
	}
}
