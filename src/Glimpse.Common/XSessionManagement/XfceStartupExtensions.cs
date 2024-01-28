using Glimpse.Common.DBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Common.XSessionManagement;

public static class XfceStartupExtensions
{
	public static async Task UseXSessionManagement(this IHost host, string installationPath)
	{
		var container = host.Services;
		await container.GetRequiredService<XSessionManager>().Register(installationPath);
	}

	public static void AddXSessionManagement(this IHostApplicationBuilder builder, string applicationId)
	{
		var services = builder.Services;
		services.AddSingleton<XSessionManager>();
		services.AddSingleton(c => new OrgXfceSessionClient(c.GetRequiredService<DBusConnections>().Session, applicationId));
		services.AddSingleton(c => new OrgXfceSessionManager(c.GetRequiredService<DBusConnections>().Session));
	}
}
