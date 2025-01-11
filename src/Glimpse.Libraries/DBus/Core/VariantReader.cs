using System.Text;
using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.DBus.Core;

public static class VariantReader
{
	public static DBusVariantItem ReadDBusVariant(this ref Reader reader)
	{
		ReadOnlySpan<byte> signature = reader.ReadSignatureAsSpan();
		SignatureReader signatureReader = new(signature);
		if (!signatureReader.TryRead(out var dBusType, out var innerSignature))
		{
			throw new InvalidOperationException("Unable to read empty variant");
		}

		return new DBusVariantItem(Encoding.UTF8.GetString(innerSignature.ToArray()), reader.ReadDBusItem(dBusType, innerSignature));
	}

	private static DBusBasicTypeItem ReadDBusBasicTypeItem(this ref Reader reader, DBusType dBusType) =>
		dBusType switch
		{
			DBusType.Byte => new DBusByteItem(reader.ReadByte()),
			DBusType.Bool => new DBusBoolItem(reader.ReadBool()),
			DBusType.Int16 => new DBusInt16Item(reader.ReadInt16()),
			DBusType.UInt16 => new DBusUInt16Item(reader.ReadUInt16()),
			DBusType.Int32 => new DBusInt32Item(reader.ReadInt32()),
			DBusType.UInt32 => new DBusUInt32Item(reader.ReadUInt32()),
			DBusType.Int64 => new DBusInt64Item(reader.ReadInt64()),
			DBusType.UInt64 => new DBusUInt64Item(reader.ReadUInt64()),
			DBusType.Double => new DBusDoubleItem(reader.ReadDouble()),
			DBusType.String => new DBusStringItem(reader.ReadString()),
			DBusType.ObjectPath => new DBusObjectPathItem(reader.ReadObjectPath()),
			DBusType.Signature => new DBusSignatureItem(new Signature(reader.ReadSignature().ToString())),
			_ => throw new ArgumentOutOfRangeException(nameof(dBusType))
		};

	private static DBusItem ReadDBusItem(this ref Reader reader, DBusType dBusType, ReadOnlySpan<byte> innerSignature)
	{
		switch (dBusType)
		{
			case DBusType.Byte:
				return new DBusByteItem(reader.ReadByte());
			case DBusType.Bool:
				return new DBusBoolItem(reader.ReadBool());
			case DBusType.Int16:
				return new DBusInt16Item(reader.ReadInt16());
			case DBusType.UInt16:
				return new DBusUInt16Item(reader.ReadUInt16());
			case DBusType.Int32:
				return new DBusInt32Item(reader.ReadInt32());
			case DBusType.UInt32:
				return new DBusUInt32Item(reader.ReadUInt32());
			case DBusType.Int64:
				return new DBusInt64Item(reader.ReadInt64());
			case DBusType.UInt64:
				return new DBusUInt64Item(reader.ReadUInt64());
			case DBusType.Double:
				return new DBusDoubleItem(reader.ReadDouble());
			case DBusType.String:
				return new DBusStringItem(reader.ReadString());
			case DBusType.ObjectPath:
				return new DBusObjectPathItem(reader.ReadObjectPath());
			case DBusType.Signature:
				return new DBusSignatureItem(new Signature(reader.ReadSignature().ToString()));
			case DBusType.Array:
				{
					SignatureReader innerSignatureReader = new(innerSignature);
					if (!innerSignatureReader.TryRead(out var innerDBusType, out var innerArraySignature))
					{
						throw new InvalidOperationException("Failed to deserialize array item");
					}

					List<DBusItem> items = new();
					var arrayEnd = reader.ReadArrayStart(innerDBusType);
					while (reader.HasNext(arrayEnd))
					{
						items.Add(reader.ReadDBusItem(innerDBusType, innerArraySignature));
					}

					return new DBusArrayItem(innerDBusType, items);
				}
			case DBusType.DictEntry:
				{
					SignatureReader innerSignatureReader = new(innerSignature);
					if (!innerSignatureReader.TryRead(out var innerKeyType, out var _) ||
						!innerSignatureReader.TryRead(out var innerValueType, out var innerValueSignature))
					{
						throw new InvalidOperationException($"Expected 2 inner types for DictEntry, got {Encoding.UTF8.GetString(innerSignature.ToArray())}");
					}

					var key = reader.ReadDBusBasicTypeItem(innerKeyType);
					var value = reader.ReadDBusItem(innerValueType, innerValueSignature);
					return new DBusDictEntryItem(key, value);
				}
			case DBusType.Struct:
				{
					reader.AlignStruct();
					List<DBusItem> items = new();
					SignatureReader innerSignatureReader = new(innerSignature);
					while (innerSignatureReader.TryRead(out var innerDBusType, out var innerStructSignature))
					{
						items.Add(reader.ReadDBusItem(innerDBusType, innerStructSignature));
					}

					return new DBusStructItem(items);
				}
			case DBusType.Variant:
				return reader.ReadDBusVariant();
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
