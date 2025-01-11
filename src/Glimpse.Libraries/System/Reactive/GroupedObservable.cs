using System.Reactive;
using System.Reactive.Linq;

namespace Glimpse.Libraries.System.Reactive;

public sealed class GroupedObservable<TKey, TValue> : ObservableBase<TValue>, IGroupedObservable<TKey, TValue>
{
	private readonly IObservable<TValue> _subject;

	public GroupedObservable(TKey key, IObservable<TValue> subject)
	{
		Key = key;
		_subject = subject;
	}

	public TKey Key { get; }

	protected override IDisposable SubscribeCore(IObserver<TValue> observer)
	{
		return _subject.Subscribe(observer);
	}
}
