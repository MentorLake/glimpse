using System.Reactive.Linq;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Libraries.Xorg.X11;
using MentorLake.Redux;
using Microsoft.Extensions.Hosting;
using DateTime = System.DateTime;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Libraries.Xorg;

internal class XorgHostedService(XLibAdaptorService xLibAdaptorService, ReduxStore store) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		xLibAdaptorService.FocusChanged.Subscribe(w => store.Dispatch(new WindowFocusedChangedAction() { WindowRef = w }));

		xLibAdaptorService.Windows.Subscribe(windowObs =>
		{
			windowObs.Take(1).Subscribe(w => store.Dispatch(new AddWindowAction(w with { CreationDate = DateTime.UtcNow })));
			windowObs.Skip(1).Subscribe(w => store.Dispatch(new UpdateWindowAction() { WindowProperties = w }));
			windowObs.TakeLast(1).Subscribe(w => store.Dispatch(new RemoveWindowAction() { WindowProperties = w }));
		});

		await xLibAdaptorService.InitializeAsync();
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
