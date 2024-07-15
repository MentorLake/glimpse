using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Common.Xfce.SessionManagement;

public static class XfceStartupExtensions
{
	public static void AddXSessionManagement(this IHostApplicationBuilder builder)
	{
		var services = builder.Services;
		services.AddSingleton<OrgXfceSessionClient>();
	}

	public static Task UseXSessionManagement(this IHost host, string installationPath, GlimpseXfceOptions options)
	{
		var sessionClient = host.Services.GetRequiredService<OrgXfceSessionClient>();
		sessionClient.Register(installationPath, options);
		return Task.CompletedTask;
	}
}
