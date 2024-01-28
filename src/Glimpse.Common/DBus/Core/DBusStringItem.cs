namespace Glimpse.Common.DBus.Core;

public class DBusStringItem : DBusBasicTypeItem
{
	public DBusStringItem(string value) => Value = value;

	public string Value { get; }

	public override string ToString()
	{
		return Value;
	}
}
