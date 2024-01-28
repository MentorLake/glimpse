namespace Glimpse.Common.DBus.Core;

public class DBusVariantItem : DBusItem
{
	public DBusVariantItem(string signature, DBusItem value)
	{
		Signature = signature;
		Value = value;
	}

	public string Signature { get; }

	public DBusItem Value { get; }
}
