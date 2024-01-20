using System.Collections;

namespace Glimpse.Freedesktop.DBus.Core;

public class DBusStructItem : DBusItem, IReadOnlyList<DBusItem>
{
	private readonly IReadOnlyList<DBusItem> _value;

	public DBusStructItem(IReadOnlyList<DBusItem> value) => _value = value;

	public IEnumerator<DBusItem> GetEnumerator() => _value.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_value).GetEnumerator();

	public int Count => _value.Count;

	public DBusItem this[int index] => _value[index];
}
