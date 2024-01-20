using Tmds.DBus.Protocol;

namespace Glimpse.Freedesktop.DBus.Core;

public class DBusObjectPathItem : DBusBasicTypeItem
{
	public DBusObjectPathItem(ObjectPath value) => Value = value;

	public ObjectPath Value { get; }
}
