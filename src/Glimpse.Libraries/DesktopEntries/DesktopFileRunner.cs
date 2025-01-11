using System.Diagnostics;
using Glimpse.Libraries.Gtk;
using MentorLake.Gio;

namespace Glimpse.Libraries.DesktopEntries;

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
		using var gDesktopFile = GDesktopAppInfoHandle.NewFromFilename(action.DesktopFilePath);
		gDesktopFile.LaunchAction(action.Id, null);
	}

	public static void Run(string command)
	{
		var startInfo = new ProcessStartInfo("setsid", command);
		startInfo.WorkingDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		Process.Start(startInfo);
	}
}
