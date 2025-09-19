using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using MentorLake.Gtk;
using MentorLake.GObject;
using MentorLake.Gtk3;

namespace Glimpse.Libraries.Xfce.SessionManagement;

public class XfceSMClientHandle : GObjectHandle
{
	public static XfceSMClientHandle Get()
	{
		return LibXfce4UIExterns.xfce_sm_client_get();
	}

	public static XfceSMClientHandle Get(string[] commandLineArgs, int restartStyle, byte priority)
	{
		var argv = new string[commandLineArgs.Length + 1];
		Array.Copy(commandLineArgs, argv, commandLineArgs.Length);
		return LibXfce4UIExterns.xfce_sm_client_get_with_argv(argv.Length - 1, argv, restartStyle, priority);
	}
}

public class XfceSMClient
{
	private readonly XfceSMClientHandle _handle;

	public XfceSMClient(XfceSMClientHandle handle)
	{
		_handle = handle;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void quit([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (DelegateSafeHandleMarshaller<GObjectHandle>))] GObjectHandle self, IntPtr user_data);

	public IObservable<Unit> Signal_Quit()
	{
		return Observable.Create((Func<IObserver<Unit>, IDisposable>) (obs =>
		{
			ulong handlerId = GObjectGlobalFunctions.SignalConnectData(_handle, "quit", Marshal.GetFunctionPointerForDelegate((quit)((_, _) => obs.OnNext(Unit.Default))), IntPtr.Zero, null, GConnectFlags.G_CONNECT_AFTER);
			return Disposable.Create((Action) (() =>
			{
				GObjectGlobalFunctions.SignalHandlerDisconnect(_handle, handlerId);
				obs.OnCompleted();
			}));
		}));
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
		var nullTerminatedArray = new string[command.Length + 1];
		Array.Copy(command, nullTerminatedArray, command.Length);
		LibXfce4UIExterns.xfce_sm_client_set_restart_command(handle, nullTerminatedArray);
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
