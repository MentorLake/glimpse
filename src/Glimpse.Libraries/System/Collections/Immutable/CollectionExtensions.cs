using System.Collections.Immutable;

namespace Glimpse.Libraries.System.Collections.Immutable;

public struct Pairing<TKey, TLeft, TRight>
{
	public TKey Key;
	public TLeft? Left;
	public TRight? Right;
}

public static class CollectionExtensions
{
	public static ImmutableList<T> Toggle<T>(this ImmutableList<T> set, T val)
	{
		return set.Contains(val) ? set.Remove(val) : set.Add(val);
	}

	public static LinkedList<Pairing<TKey, TValueLeft, TValueRight>> Diff<TKey, TValueLeft, TValueRight>(this ImmutableDictionary<TKey, TValueLeft> left, ImmutableDictionary<TKey, TValueRight> right)
	{
		var pairings = new LinkedList<Pairing<TKey, TValueLeft, TValueRight>>();

		foreach (var key in left.Keys)
		{
			if (!right.ContainsKey(key))
			{
				pairings.AddLast(new Pairing<TKey, TValueLeft, TValueRight>() { Key = key, Left = left[key] });
			}
			else
			{
				pairings.AddLast(new Pairing<TKey, TValueLeft, TValueRight>() { Key = key, Left = left[key], Right = right[key] });
			}
		}

		foreach (var key in right.Keys)
		{
			if (!left.ContainsKey(key))
			{
				pairings.AddLast(new Pairing<TKey, TValueLeft, TValueRight>() { Key = key, Right = right[key] });
			}
		}

		return pairings;
	}
}
