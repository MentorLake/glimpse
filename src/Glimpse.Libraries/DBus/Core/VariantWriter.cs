using System.Text;
using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.DBus.Core;

public static class VariantWriter
{
	public static void WriteDBusVariant(this ref MessageWriter writer, DBusVariantItem value)
	{
		writer.WriteSignature(Encoding.UTF8.GetBytes(value.Signature).AsSpan());
		writer.WriteDBusItem(value.Value);
	}

	public static void WriteDBusItem(this ref MessageWriter writer, DBusItem value)
	{
		switch (value)
		{
			case DBusVariantItem variantItem:
				writer.WriteDBusVariant(variantItem);
				break;
			case DBusByteItem byteItem:
				writer.WriteByte(byteItem.Value);
				break;
			case DBusBoolItem boolItem:
				writer.WriteBool(boolItem.Value);
				break;
			case DBusInt16Item int16Item:
				writer.WriteInt16(int16Item.Value);
				break;
			case DBusUInt16Item uInt16Item:
				writer.WriteUInt16(uInt16Item.Value);
				break;
			case DBusInt32Item int32Item:
				writer.WriteInt32(int32Item.Value);
				break;
			case DBusUInt32Item uInt32Item:
				writer.WriteUInt32(uInt32Item.Value);
				break;
			case DBusInt64Item int64Item:
				writer.WriteInt64(int64Item.Value);
				break;
			case DBusUInt64Item uInt64Item:
				writer.WriteUInt64(uInt64Item.Value);
				break;
			case DBusDoubleItem doubleItem:
				writer.WriteDouble(doubleItem.Value);
				break;
			case DBusStringItem stringItem:
				writer.WriteString(stringItem.Value);
				break;
			case DBusObjectPathItem objectPathItem:
				writer.WriteObjectPath(objectPathItem.Value);
				break;
			case DBusSignatureItem signatureItem:
				writer.WriteSignature(signatureItem.Value.ToString());
				break;
			case DBusArrayItem arrayItem:
				var arrayStart = writer.WriteArrayStart(arrayItem.ArrayType);
				foreach (var item in arrayItem)
				{
					writer.WriteDBusItem(item);
				}

				writer.WriteArrayEnd(arrayStart);
				break;
			case DBusDictEntryItem dictEntryItem:
				writer.WriteStructureStart();
				writer.WriteDBusItem(dictEntryItem.Key);
				writer.WriteDBusItem(dictEntryItem.Value);
				break;
			case DBusStructItem structItem:
				writer.WriteStructureStart();
				foreach (var item in structItem)
				{
					writer.WriteDBusItem(item);
				}

				break;
			case DBusByteArrayItem byteArrayItem:
				var byteArrayStart = writer.WriteArrayStart(DBusType.Byte);
				foreach (var item in byteArrayItem)
				{
					writer.WriteByte(item);
				}

				writer.WriteArrayEnd(byteArrayStart);
				break;
		}
	}
}
