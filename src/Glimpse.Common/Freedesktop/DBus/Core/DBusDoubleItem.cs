namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusDoubleItem : DBusBasicTypeItem
{
	public DBusDoubleItem(double value) => Value = value;

	public double Value { get; }
}
