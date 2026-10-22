using Glimpse.Libraries.Xorg.State;
using Glimpse.Libraries.Xorg.X11;
using DateTime = System.DateTime;

namespace Glimpse.Libraries.Xorg;

internal class XorgHostedService(
	X11EventQueue queue,
	XLibAdaptorService xLibAdaptorService,
	XClipboardServer clipboardServer,
	ReduxStore store,
	IHostApplicationLifetime lifetime) : IHostedService
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

		XLib.XInitThreads();
		var display = XLib.XOpenDisplay(0);
		xLibAdaptorService.Initialize(display);
		clipboardServer.Initialize(display);
		queue.Start(display);

		lifetime.ApplicationStopping.Register(() =>
		{
			xLibAdaptorService.Dispose();
			clipboardServer.Dispose();
			XLib.XCloseDisplay(display);
		});
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
