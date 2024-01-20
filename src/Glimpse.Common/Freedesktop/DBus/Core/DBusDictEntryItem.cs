namespace Glimpse.Freedesktop.DBus.Core;

public class DBusDictEntryItem : DBusItem
{
	public DBusDictEntryItem(DBusBasicTypeItem key, DBusItem value)
	{
		Key = key;
		Value = value;
	}

	public DBusBasicTypeItem Key { get; }

	public DBusItem Value { get; }
}
