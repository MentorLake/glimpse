using System.Text.Json;
using System.Text.Json.Serialization;

namespace Glimpse.Libraries.System.Text.Json;

public static class JsonSerializerExtensions
{
	extension(JsonSerializer)
	{
		public static bool TryDeserialize<T>(string json, JsonSerializerContext context, out T deserializedObject) where T : class
		{
			deserializedObject = null;

			try
			{
				deserializedObject = JsonSerializer.Deserialize(json, typeof(T), context) as T;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
	}
}
