using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tmds.DBus.Protocol;

namespace Glimpse.Libraries.DBus;

public static class DBusStartupExtensions
{
	public static async Task UseDBus(this IHost host)
	{
		var container = host.Services;
		var dbusConnections = container.GetRequiredService<DBusConnections>();
		await dbusConnections.Session.ConnectAsync();
		await dbusConnections.System.ConnectAsync();
	}

	public static void AddDBus(this IHostApplicationBuilder builder)
	{
		var services = builder.Services;
		services.AddSingleton<IntrospectionService>();
		services.AddSingleton(c => new OrgFreedesktopDBus(c.GetRequiredService<DBusConnections>().Session, Connection.DBusServiceName, Connection.DBusObjectPath));
		services.AddSingleton(new DBusConnections() { Session = new Connection(Address.Session!), System = new Connection(Address.System!), });
	}
}
