using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Glimpse.Common.Images;

public class GlimpseImageJsonConverter : JsonConverter<IGlimpseImage>
{
	public override IGlimpseImage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var imageJson = JsonNode.Parse(ref reader, new JsonNodeOptions() { PropertyNameCaseInsensitive = true }) as JsonObject;
		imageJson.TryGetPropertyValue("Data", out var dataNode);

		if (string.IsNullOrEmpty(dataNode.GetValue<string>()))
		{
			return null;
		}

		return GlimpseImageFactory.From(Convert.FromBase64String(dataNode.GetValue<string>()));
	}

	public override void Write(Utf8JsonWriter writer, IGlimpseImage value, JsonSerializerOptions options)
	{
		var pixbuf = value.Pixbuf;
		var pngByteBuffer = pixbuf.SaveToBuffer("png");
		writer.WriteStartObject();
		writer.WriteBase64String("Data", pngByteBuffer);
		writer.WriteNumber("Width", pixbuf.Width);
		writer.WriteNumber("Height", pixbuf.Height);
		writer.WriteEndObject();
	}
}
