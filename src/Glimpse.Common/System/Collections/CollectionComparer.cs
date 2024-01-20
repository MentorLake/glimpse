namespace Glimpse.Common.System.Collections;

public static class CollectionComparer
{
	public static bool Sequence<T>(IEnumerable<T> x, IEnumerable<T> y, Func<T, T, bool> elementComparer)
	{
		return Sequence(x, y, EqualityComparer<T>.Create(elementComparer));
	}

	public static bool Sequence<T>(IEnumerable<T> x, IEnumerable<T> y, IEqualityComparer<T> elementComparer = null)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		elementComparer ??= EqualityComparer<T>.Default;
		return x.SequenceEqual(y, elementComparer);
	}
}
