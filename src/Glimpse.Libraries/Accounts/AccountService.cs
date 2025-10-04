using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.DBus;
using MentorLake.Redux;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Libraries.Accounts;

public class AccountService(ReduxStore store, OrgFreedesktopAccounts freedesktopAccounts)
{
	public async Task InitializeAsync(DBusConnections dBusConnections)
	{
		var userObjectPath = await freedesktopAccounts.FindUserByNameAsync(Environment.UserName);
		var service = new AccountsDbusServiceFactory(dBusConnections.System, "org.freedesktop.Accounts");
		var userService = service.CreateUser(userObjectPath);

		var subject = new Subject<Unit>();
		_ = await userService.WatchPropertiesChangedAsync((_, _) => subject.OnNext(Unit.Default));

		Observable
			.Return(Observable.FromAsync(() => userService.GetPropertiesAsync()))
			.Concat(subject.Select(_ => Observable.FromAsync(() => userService.GetPropertiesAsync())))
			.Concat()
			.Subscribe(p =>
			{
				store.Dispatch(new UpdateUserAction() { UserName = p.UserName, IconPath = p.IconFile });
			});
	}
}
