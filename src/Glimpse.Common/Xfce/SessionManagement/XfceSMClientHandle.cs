using GLib;
using Microsoft.Win32.SafeHandles;

namespace Glimpse.Common.Xfce.SessionManagement;

public class XfceSMClientHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	public XfceSMClientHandle() : base(true) { }

	protected override bool ReleaseHandle() => true;

	public static XfceSMClientHandle Get()
	{
		return LibXfce4UIExterns.xfce_sm_client_get();
	}

	public static XfceSMClientHandle Get(string[] commandLineArgs, int restartStyle, byte priority)
	{
		return LibXfce4UIExterns.xfce_sm_client_get_with_argv(commandLineArgs.Length, commandLineArgs, restartStyle, priority);
	}
}

public class XfceSMClient : GLib.Object
{
	private readonly XfceSMClientHandle _handle;

	public XfceSMClient(XfceSMClientHandle handle) : base(handle.DangerousGetHandle())
	{
		_handle = handle;
	}

	[Signal("quit")]
	public event EventHandler Quit
	{
		add => AddSignalHandler("quit", (Delegate) value);
		remove => RemoveSignalHandler("quit", (Delegate) value);
	}

	public void SetRestartStyle(int restartStyle)
	{
		_handle.SetRestartStyle(restartStyle);
	}

	public void SetPriority(byte priority)
	{
		_handle.SetPriority(priority);
	}

	public bool Connect()
	{
		return _handle.Connect();
	}

	public void SetRestartCommand(string[] command)
	{
		_handle.SetRestartCommand(command);
	}

	public void SetCurrentDirectory(string directory)
	{
		_handle.SetCurrentDirectory(directory);
	}

	public string GetClientId()
	{
		return _handle.GetClientId();
	}

	public void Disconnect()
	{
		_handle.Disconnect();
	}
}

internal static class XfceSMClientHandleExtensions
{
	public static void SetRestartStyle(this XfceSMClientHandle handle, int restartStyle)
	{
		LibXfce4UIExterns.xfce_sm_client_set_restart_style(handle, restartStyle);
	}

	public static void SetPriority(this XfceSMClientHandle handle, byte priority)
	{
		LibXfce4UIExterns.xfce_sm_client_set_priority(handle, priority);
	}

	public static bool Connect(this XfceSMClientHandle handle)
	{
		LibXfce4UIExterns.xfce_sm_client_connect(handle, out _);
		return true;
	}

	public static void SetRestartCommand(this XfceSMClientHandle handle, string[] command)
	{
		LibXfce4UIExterns.xfce_sm_client_set_restart_command(handle, command);
	}

	public static void SetCurrentDirectory(this XfceSMClientHandle handle, string directory)
	{
		LibXfce4UIExterns.xfce_sm_client_set_current_directory(handle, directory);
	}

	public static string GetClientId(this XfceSMClientHandle handle)
	{
		return LibXfce4UIExterns.xfce_sm_client_get_client_id(handle);
	}

	public static void Disconnect(this XfceSMClientHandle handle)
	{
		LibXfce4UIExterns.xfce_sm_client_disconnect(handle);
	}
}
