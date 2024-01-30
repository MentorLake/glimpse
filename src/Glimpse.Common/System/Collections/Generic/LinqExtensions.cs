namespace Glimpse.Common.System.Collections.Generic;

public static class LinqExtensions
{
	public static IEnumerable<T> SkipConsecutiveDuplicates<T>(this IEnumerable<T> source, Func<T, T, bool> isSameAsLast)
	{
		var lastElement = default(T);
		var pastFirstElement = false;

		foreach (var element in source)
		{
			if (!pastFirstElement || !isSameAsLast(lastElement, element))
			{
				pastFirstElement = true;
				lastElement = element;
				yield return element;
			}
		}
	}
}
