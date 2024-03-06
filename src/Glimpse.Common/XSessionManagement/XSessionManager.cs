using Glimpse.Common.DBus.Core;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.XSessionManagement;

public class XSessionManager(OrgXfceSessionManager xfceSessionManager, OrgXfceSessionClient xfceSessionClient)
{
	public Task Register(string assemblyPath)
	{
		_ = Task.Run(async () =>
		{
			while (!await TryRegister(assemblyPath))
			{
				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		});

		return Task.CompletedTask;
	}

	private async Task<bool> TryRegister(string assemblyPath)
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
			return true;
		}
		catch (Exception e)
		{
			return false;
		}
	}
}
