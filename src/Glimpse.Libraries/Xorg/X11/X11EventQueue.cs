namespace Glimpse.Libraries.Xorg.X11;

internal class X11EventQueue(IHostApplicationLifetime applicationLifetime)
{
	private readonly Subject<(XAnyEvent someEvent, IntPtr eventPointer)> _events = new();

	public IObservable<(XAnyEvent someEvent, IntPtr eventPointer)> Events => _events;

	public void Start(ulong display)
	{
		var cancellationToken = applicationLifetime.ApplicationStopping;

		_ = Task.Run(() =>
		{
			var eventPointer = Marshal.AllocHGlobal(24 * sizeof(long));

			while (!cancellationToken.IsCancellationRequested)
			{
				XLib.XNextEvent(display, eventPointer);
				var someEvent = Marshal.PtrToStructure<XAnyEvent>(eventPointer);
				_events.OnNext((someEvent, eventPointer));
			}

			XLib.XFree(eventPointer);
		}, cancellationToken);
	}
}
