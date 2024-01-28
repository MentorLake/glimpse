using Tmds.DBus.Protocol;

namespace Glimpse.Common.DBus.Core;

public static class WriterExtensions
{
	public static void WriteDictionary_aesv(this ref MessageWriter writer, Dictionary<string, DBusVariantItem> values)
	{
		var arrayStart = writer.WriteArrayStart(DBusType.Struct);
		foreach (var value in values)
		{
			writer.WriteStructureStart();
			writer.WriteString(value.Key);
			writer.WriteDBusVariant(value.Value);
		}

		writer.WriteArrayEnd(arrayStart);
	}

	public static void WriteArray_as(this ref MessageWriter writer, string[] values)
	{
		var arrayStart = writer.WriteArrayStart(DBusType.String);
		foreach (var value in values)
		{
			writer.WriteString(value);
		}

		writer.WriteArrayEnd(arrayStart);
	}

	public static void WriteArray_ai(this ref MessageWriter writer, int[] values)
	{
		var arrayStart = writer.WriteArrayStart(DBusType.Int32);
		foreach (var value in values)
		{
			writer.WriteInt32(value);
		}

		writer.WriteArrayEnd(arrayStart);
	}

	public static void WriteStruct_risvuz(this ref MessageWriter writer, (int, string, DBusVariantItem, uint) value)
	{
		writer.WriteStructureStart();
		writer.WriteInt32(value.Item1);
		writer.WriteString(value.Item2);
		writer.WriteDBusVariant(value.Item3);
		writer.WriteUInt32(value.Item4);
	}

	public static void WriteArray_arisvuz(this ref MessageWriter writer, (int, string, DBusVariantItem, uint)[] values)
	{
		var arrayStart = writer.WriteArrayStart(DBusType.Struct);
		foreach (var value in values)
		{
			writer.WriteStruct_risvuz(value);
		}

		writer.WriteArrayEnd(arrayStart);
	}

	public static void WriteDictionary_aess(this ref MessageWriter writer, Dictionary<string, string> values)
	{
		var arrayStart = writer.WriteArrayStart(DBusType.Struct);
		foreach (var value in values)
		{
			writer.WriteStructureStart();
			writer.WriteString(value.Key);
			writer.WriteString(value.Value);
		}

		writer.WriteArrayEnd(arrayStart);
	}

	public static void WriteArray_aaess(this ref MessageWriter writer, Dictionary<string, string>[] values)
	{
		ArrayStart arrayStart = writer.WriteArrayStart(DBusType.DictEntry);
		foreach (Dictionary<string, string> value in values)
			writer.WriteDictionary_aess(value);
		writer.WriteArrayEnd(arrayStart);
	}
}
