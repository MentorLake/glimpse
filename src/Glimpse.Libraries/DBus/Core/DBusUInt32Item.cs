namespace Glimpse.Libraries.DBus.Core;

public class DBusUInt32Item : DBusBasicTypeItem
{
	public DBusUInt32Item(uint value) => Value = value;

	public uint Value { get; }
}
