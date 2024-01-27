using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusObjectPathItem : DBusBasicTypeItem
{
	public DBusObjectPathItem(ObjectPath value) => Value = value;

	public ObjectPath Value { get; }
}
