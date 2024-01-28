namespace Glimpse.Common.DBus.Core;

public class DBusInt16Item : DBusBasicTypeItem
{
	public DBusInt16Item(short value) => Value = value;

	public short Value { get; }
}
