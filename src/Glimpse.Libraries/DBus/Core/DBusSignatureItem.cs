using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.DBus.Core;

public class DBusSignatureItem : DBusBasicTypeItem
{
	public DBusSignatureItem(Signature value) => Value = value;

	public Signature Value { get; }
}
