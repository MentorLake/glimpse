using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Glimpse.Libraries.Xfce.SessionManagement;

public class OrgXfceSessionClient(IHostApplicationLifetime applicationLifetime, ILogger<OrgXfceSessionClient> logger)
{
	public void Register(string assemblyPath, GlimpseXfceOptions options)
	{
		var restartStyle = Enum.TryParse(options.RestartStyle, true, out LibXfce4UIExterns.RestartStyle parsedRestartStyle)
			? parsedRestartStyle
			: LibXfce4UIExterns.RestartStyle.IfRunning;

		var client = new XfceSMClient(XfceSMClientHandle.Get());
		client.SetRestartStyle((int) restartStyle);
		client.SetPriority(25);

		if (Environment.GetCommandLineArgs().Any(a => a.Contains("sm-client-id")))
		{
			client = new XfceSMClient(XfceSMClientHandle.Get(Environment.GetCommandLineArgs(), (int)restartStyle, 25));
		}

		client.SetRestartCommand([assemblyPath]);
		client.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));
		client.Signal_Quit().Subscribe(_ => applicationLifetime.StopApplication());
		client.Connect();
		logger.LogInformation($"XfceSessionId: {client.GetClientId()}");
		logger.LogInformation($"AssemblyPath: {assemblyPath}");
		logger.LogInformation($"RestartStyle: {restartStyle.ToString()}");
	}
}
