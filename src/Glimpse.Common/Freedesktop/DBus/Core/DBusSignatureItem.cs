using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusSignatureItem : DBusBasicTypeItem
{
	public DBusSignatureItem(Signature value) => Value = value;

	public Signature Value { get; }
}
