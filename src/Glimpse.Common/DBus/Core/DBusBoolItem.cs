namespace Glimpse.Common.DBus.Core;

public class DBusBoolItem : DBusBasicTypeItem
{
	public DBusBoolItem(bool value) => Value = value;

	public bool Value { get; }
}
