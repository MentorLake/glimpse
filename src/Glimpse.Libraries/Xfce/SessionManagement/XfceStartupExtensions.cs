using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Libraries.Xfce.SessionManagement;

public static class XfceStartupExtensions
{
	public static void AddXSessionManagement(this IHostApplicationBuilder builder)
	{
		var services = builder.Services;
		services.AddSingleton<OrgXfceSessionClient>();
	}

	public static Task UseXSessionManagement(this IHost host)
	{
		return Task.CompletedTask;
	}
}
