namespace Glimpse.Libraries.System.Reactive;

public static class ReactiveExtensions
{
	public static IObservable<T> DistinctUntilChanged<T>(this IObservable<T> obs, Func<T, T, bool> comparison)
	{
		return obs.DistinctUntilChanged(FuncEqualityComparer<T>.Create(comparison));
	}

	public static IObservable<IGroupedObservable<TKey, TValue>> RemoveIndex<TKey, TValue>(
		this IObservable<IGroupedObservable<TKey, (TValue, int)>> source)
	{
		return source.Select(s => new GroupedObservable<TKey, TValue>(s.Key, s.Select(i => i.Item1)));
	}
}
