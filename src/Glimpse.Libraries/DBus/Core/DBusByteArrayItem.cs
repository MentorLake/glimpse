using System.Collections;

namespace Glimpse.Libraries.DBus.Core;

public class DBusByteArrayItem : DBusItem, IReadOnlyList<byte>
{
	private readonly byte[] _value;

	public DBusByteArrayItem(byte[] value) => _value = value;

	public IEnumerator<byte> GetEnumerator() => _value.AsEnumerable().GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_value).GetEnumerator();

	public int Count => _value.Length;

	public byte this[int index] => _value[index];

	public byte[] Value => _value;
}
