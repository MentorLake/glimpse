using System.Diagnostics;
using Glimpse.Common.Gtk;

namespace Glimpse.Common.Freedesktop.DesktopEntries;

public class DesktopFileRunner
{
	public static void Run(DesktopFile desktopFile)
	{
		var startInfo = new ProcessStartInfo("setsid", "xdg-open " + desktopFile.FilePath);
		startInfo.WorkingDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		Process.Start(startInfo);
	}

	public static void Run(DesktopFileAction action)
	{
		var gDesktopFile = LibGdk3Interop.g_desktop_app_info_new_from_filename(action.DesktopFilePath);
		LibGdk3Interop.g_desktop_app_info_launch_action(gDesktopFile, action.Id, IntPtr.Zero);
	}

	public static void Run(string command)
	{
		var startInfo = new ProcessStartInfo("setsid", command);
		startInfo.WorkingDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		Process.Start(startInfo);
	}
}
