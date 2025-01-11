using System.Reactive.Linq;

namespace Glimpse.Libraries.System.Reactive;

public static class TimerFactory
{
	public static readonly IObservable<DateTime> OneSecondTimer = Observable.Create<DateTime>(async (observer, cancellationToken) =>
	{
		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
		cancellationToken.Register(observer.OnCompleted);

		while (await timer.WaitForNextTickAsync(cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			observer.OnNext(DateTime.Now);
		}
	});
}
