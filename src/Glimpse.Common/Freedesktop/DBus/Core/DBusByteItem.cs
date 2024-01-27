namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusByteItem : DBusBasicTypeItem
{
	public DBusByteItem(byte value) => Value = value;

	public byte Value { get; }
}
