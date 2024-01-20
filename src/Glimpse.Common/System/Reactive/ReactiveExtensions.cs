using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Glimpse.Common.System.Reactive;

public static class ReactiveExtensions
{
	public static IObservable<T> DistinctUntilChanged<T>(this IObservable<T> obs, Func<T, T, bool> comparison)
	{
		return obs.DistinctUntilChanged(FuncEqualityComparer<T>.Create(comparison));
	}

	public static IObservable<IGroupedObservable<TKey, TValue>> RemoveIndex<TKey, TValue>(this IObservable<IGroupedObservable<TKey, (TValue, int)>> source)
	{
		return source.Select(s => new GroupedObservable<TKey, TValue>(s.Key, s.Select(i => i.Item1)));
	}

	public static IObservable<IGroupedObservable<TValue, (TValue, int)>> UnbundleMany<TValue, TKey>(this IObservable<IEnumerable<TValue>> source, Func<TValue, TKey> keySelector) where TKey : IEquatable<TKey>
	{
		return Observable.Create<IGroupedObservable<TValue, (TValue, int)>>(obs =>
		{
			var groupSubjects = new Dictionary<TKey, Subject<(TValue, int)>>();

			return source.Subscribe(elements =>
				{
					var elementList = new List<TValue>();
					elementList.AddRange(elements);

					for (var i = 0; i < elementList.Count; i++)
					{
						var e = elementList[i];
						var key = keySelector(e);

						if (!groupSubjects.ContainsKey(key))
						{
							groupSubjects.Add(key, new Subject<(TValue, int)>());
							obs.OnNext(new GroupedObservable<TValue, (TValue, int)>(e, groupSubjects[key]));
						}

						groupSubjects[key].OnNext((e, i));
					}

					foreach (var key in groupSubjects.Keys)
					{
						if (elements.All(e => !keySelector(e).Equals(key)))
						{
							groupSubjects[key].OnCompleted();
							groupSubjects.Remove(key);
						}
					}
				},
				e =>
				{
					obs.OnError(e);
					foreach (var s in groupSubjects.Values) s.OnError(e);
				},
				() =>
				{
					foreach (var s in groupSubjects.Values) s.OnCompleted();
					obs.OnCompleted();
				});
		});
	}
}
