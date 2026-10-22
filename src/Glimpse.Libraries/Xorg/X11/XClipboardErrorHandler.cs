namespace Glimpse.Libraries.Xorg.X11;

internal class XClipboardErrorHandler(ILogger logger)
{
	private XErrorHandlerDelegate _handler;
	private XErrorHandlerDelegate _previous;

	public void Install()
	{
		_handler = Handle;
		_previous = Marshal.GetDelegateForFunctionPointer<XErrorHandlerDelegate>(XLib.XSetErrorHandler(_handler));
		GC.KeepAlive(_handler);
	}

	public void Uninstall()
	{
		if (_handler == null) return;
		XLib.XSetErrorHandler(_previous);
		_handler = null;
	}

	private int Handle(IntPtr display, ref XErrorEvent errorEvent)
	{
		// Selection peers routinely die between a request and its reply. A BadWindow or
		// BadAtom here is expected, and the default handler would terminate the process.
		logger.LogDebug("X error {ErrorCode} on request {RequestCode} for resource {ResourceId}.", errorEvent.error_code, errorEvent.request_code, errorEvent.resourceid);
		return 0;
	}
}
