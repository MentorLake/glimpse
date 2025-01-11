using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Libraries.System.Reactive;

public static class TimerStartupExtensions
{
	public static void AddReactiveTimers(this IHostApplicationBuilder builder)
	{
		builder.Services.AddKeyedSingleton(Timers.OneSecond, (c, _) =>
		{
			var host1 = c.GetRequiredService<IHostApplicationLifetime>();
			var shuttingDown = Observable.Create<Unit>(obs => host1.ApplicationStopping.Register(() => obs.OnNext(Unit.Default)));
			return TimerFactory.OneSecondTimer.TakeUntil(shuttingDown).Publish().AutoConnect();
		});
	}
}
