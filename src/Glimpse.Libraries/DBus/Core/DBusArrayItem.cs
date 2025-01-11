using System.Collections;
using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.DBus.Core;

public class DBusArrayItem : DBusItem, IReadOnlyList<DBusItem>
{
	private readonly IReadOnlyList<DBusItem> _value;

	public DBusArrayItem(DBusType arrayType, IReadOnlyList<DBusItem> value)
	{
		ArrayType = arrayType;
		_value = value;
	}

	public DBusType ArrayType { get; }

	public IEnumerator<DBusItem> GetEnumerator() => _value.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_value).GetEnumerator();

	public int Count => _value.Count;

	public DBusItem this[int index] => _value[index];
}
