namespace Glimpse.Libraries.DBus.Core;

public class DBusInt64Item : DBusBasicTypeItem
{
	public DBusInt64Item(long value) => Value = value;

	public long Value { get; }
}
