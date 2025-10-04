using Glimpse.Libraries.Microsoft.Extensions;
using Glimpse.Libraries.DBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Libraries.Accounts;

public static class AccountsStartupExtensions
{
	public static async Task UseAccounts(this IHost host)
	{
		var container = host.Services;
		var dbusConnections = container.GetRequiredService<DBusConnections>();
		await container.GetRequiredService<AccountService>().InitializeAsync(dbusConnections);
	}

	public static void AddAccounts(this IHostApplicationBuilder builder)
	{
		var services = builder.Services;
		services.AddInstance(AccountReducers.AllReducers);
		services.AddSingleton<AccountService>();
	}
}
