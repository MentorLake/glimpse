﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Xfce.SessionManagement;

public class OrgXfceSessionClient(IHostApplicationLifetime applicationLifetime, ILogger<OrgXfceSessionClient> logger)
{
	private readonly XfceSMClient _client = new(XfceSMClientHandle.Get());

	public void Register(string assemblyPath, GlimpseXfceOptions options)
	{
		var restartStyle = Enum.TryParse(options.RestartStyle, true, out LibXfce4UIExterns.RestartStyle parsedRestartStyle)
			? parsedRestartStyle
			: LibXfce4UIExterns.RestartStyle.IfRunning;

		_client.SetRestartStyle((int) restartStyle);
		_client.SetPriority(25);
		_client.SetRestartCommand([Path.GetFileName(assemblyPath)]);
		_client.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath));
		_client.Events().Quit.Subscribe(_ => applicationLifetime.StopApplication());
		_client.Connect();
		logger.LogInformation($"XfceSessionId: {_client.GetClientId()}");
	}
}
