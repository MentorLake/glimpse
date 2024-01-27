using Glimpse.Common.Freedesktop.DBus.Core;
using Glimpse.Common.Freedesktop.DBus.Interfaces;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.Freedesktop.DBus;

public class XSessionManager(OrgXfceSessionManager xfceSessionManager, OrgXfceSessionClient xfceSessionClient)
{
	public async Task Register(string assemblyPath)
	{
		try
		{
			await xfceSessionManager.RegisterClientAsync("org_glimpse", "");

			await xfceSessionClient.SetSmPropertiesAsync(new()
			{
				["_GSM_Priority"] = new("y", new DBusByteItem(25)),
				["RestartCommand"] = new("as", new DBusArrayItem(DBusType.String, new DBusStringItem[] { new(assemblyPath) })),
				["UserID"] = new("s", new DBusStringItem(Environment.UserName)),
				["RestartStyleHint"] = new("y", new DBusByteItem(0))
			});

			#if !DEBUG
			await xfceSessionClient.SetSmPropertiesAsync(new()
			{
				["RestartStyleHint"] = new("y", new DBusByteItem(2))
			});
			#endif
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

	}
}
