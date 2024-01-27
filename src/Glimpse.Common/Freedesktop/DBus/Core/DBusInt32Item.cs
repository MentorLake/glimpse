namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusInt32Item : DBusBasicTypeItem
{
	public DBusInt32Item(int value) => Value = value;

	public int Value { get; }
}
