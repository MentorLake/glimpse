namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusUInt16Item : DBusBasicTypeItem
{
	public DBusUInt16Item(ushort value) => Value = value;

	public ushort Value { get; }
}
