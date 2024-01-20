using System.Diagnostics;
using System.Reactive.Linq;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse;

public class Installation
{
	public const string InstallScriptResourceName = "install.sh";
	public const string UninstallScriptResourceName = "uninstall.sh";
	public const string DefaultInstallPath = "~/.local/bin/Glimpse";
	private const string InstallScriptTmpPath = "/tmp/glimpse-{scriptName}.sh";

	public static void RunScript(string scriptName)
	{
		try
		{
			using var resourceStream = typeof(Installation).Assembly.GetManifestResourceStream(scriptName);
			using var reader = new StreamReader(resourceStream);
			var script = reader.ReadToEnd();
			var tempScriptPath = InstallScriptTmpPath.Replace("{scriptName}", scriptName);

			File.WriteAllText(tempScriptPath, script);
			Process.Start("/bin/bash", $"-c \"chmod +x {tempScriptPath}\"")?.WaitForExit();

			using var p = new Process();
			p.EnableRaisingEvents = true;
			p.StartInfo = new ProcessStartInfo { FileName = "/bin/bash", UseShellExecute = false, Arguments = $"-c \"{tempScriptPath}\"", };

			p.Events().OutputDataReceived.TakeUntil(p.Events().Exited.Take(1)).Subscribe(a => Console.Write(a.Data));
			p.Start();
			p.WaitForExit();
		}
		finally
		{
			if (File.Exists(InstallScriptTmpPath))
			{
				File.Delete(InstallScriptTmpPath);
			}
		}
	}
}
