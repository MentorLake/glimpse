using System.Reactive.Linq;
using Glimpse.Common.DBus;
using MentorLake.Redux;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Common.Accounts;

public class AccountService(ReduxStore store, OrgFreedesktopAccounts freedesktopAccounts)
{
	public async Task InitializeAsync(DBusConnections dBusConnections)
	{
		var userObjectPath = await freedesktopAccounts.FindUserByNameAsync(Environment.UserName);
		var userService = new OrgFreedesktopAccountsUser(dBusConnections.System, "org.freedesktop.Accounts", userObjectPath);

		Observable
			.Return(await userService.GetAllPropertiesAsync())
			.Concat(userService.PropertiesChanged)
			.Subscribe(p =>
			{
				store.Dispatch(new UpdateUserAction() { UserName = p.UserName, IconPath = p.IconFile });
			});
	}
}
