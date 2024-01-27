using Glimpse.Common.Freedesktop.Accounts;
using Glimpse.Common.Freedesktop.DBus;
using Glimpse.Common.Freedesktop.DBus.Interfaces;
using Glimpse.Common.Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop;

public static class FreedesktopStartupExtensions
{
	public static async Task UseFreedesktop(this IHost host, string installationPath)
	{
		var container = host.Services;
		var dbusConnections = container.GetRequiredService<DBusConnections>();
		await dbusConnections.Session.ConnectAsync();
		await dbusConnections.System.ConnectAsync();
		await container.GetRequiredService<AccountService>().InitializeAsync(dbusConnections);
		await container.GetRequiredService<XSessionManager>().Register(installationPath);
	}

	public static void AddFreedesktop(this IHostApplicationBuilder builder)
	{
		var services = builder.Services;
		services.AddInstance(AccountReducers.AllReducers);
		services.AddSingleton<AccountService>();
		services.AddSingleton<IntrospectionService>();
		services.AddSingleton<OrgFreedesktopAccounts>();
		services.AddSingleton<OrgKdeStatusNotifierWatcher>();
		services.AddSingleton<XSessionManager>();
		services.AddSingleton(c => new OrgXfceSessionClient(c.GetRequiredService<DBusConnections>().Session, "org_glimpse"));
		services.AddSingleton(c => new OrgFreedesktopDBus(c.GetRequiredService<DBusConnections>().Session, Connection.DBusServiceName, Connection.DBusObjectPath));
		services.AddSingleton(c => new OrgXfceSessionManager(c.GetRequiredService<DBusConnections>().Session));
		services.AddSingleton(new DBusConnections() { Session = new Connection(Address.Session!), System = new Connection(Address.System!), });
	}
}
