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
		services.AddSingleton(c => new OrgFreedesktopDBus(c.GetRequiredService<DBusConnections>().Session, "org.freedesktop.DBus", "/org/freedesktop/DBus"));
		services.AddSingleton(new DBusConnections() { Session = new DBusConnection(DBusAddress.Session!), System = new DBusConnection(DBusAddress.System!), });
	}
}
