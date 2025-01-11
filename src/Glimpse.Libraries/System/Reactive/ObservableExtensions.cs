using System.Diagnostics;

namespace Glimpse.Libraries.System.Reactive;

public static class ObservableExtensions
{
	public static IDisposable SubscribeDebug<T>(this IObservable<T> observable, Action<T> onNext)
	{
		return observable.Subscribe(val =>
		{
			try
			{
				onNext(val);
			}
			catch (Exception ex)
			{
				#if DEBUG
				Debugger.Break();
				#endif
				Console.WriteLine(ex);
			}
		});
	}
}
