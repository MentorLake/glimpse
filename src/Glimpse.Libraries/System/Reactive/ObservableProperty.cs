using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Glimpse.Libraries.System.Reactive;

public class ObservableProperty<T> : IDisposable, IObservable<T>
{
	private readonly BehaviorSubject<IObservable<T>> _subject;
	private readonly IDisposable _subscription;
	private readonly IObservable<T> _source;

	public ObservableProperty(T initialValue)
	{
		_subject = new(Observable.Return(initialValue));
		var obs = _subject.Switch().Replay(1);
		_subscription = obs.Connect();
		_source = obs;
	}

	public void UpdateSource(IObservable<T> newSource) => _subject.OnNext(newSource);
	public void Dispose() => _subscription.Dispose();
	public IDisposable Subscribe(IObserver<T> observer) => _source.Subscribe(observer);
}
