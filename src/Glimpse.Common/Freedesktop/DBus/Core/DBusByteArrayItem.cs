using System.Collections;

namespace Glimpse.Common.Freedesktop.DBus.Core;

public class DBusByteArrayItem : DBusItem, IReadOnlyList<byte>
{
	private readonly IReadOnlyList<byte> _value;

	public DBusByteArrayItem(IReadOnlyList<byte> value) => _value = value;

	public IEnumerator<byte> GetEnumerator() => _value.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_value).GetEnumerator();

	public int Count => _value.Count;

	public byte this[int index] => _value[index];
}
