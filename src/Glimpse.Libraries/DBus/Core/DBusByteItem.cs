namespace Glimpse.Libraries.DBus.Core;

public class DBusByteItem : DBusBasicTypeItem
{
	public DBusByteItem(byte value) => Value = value;

	public byte Value { get; }
}
