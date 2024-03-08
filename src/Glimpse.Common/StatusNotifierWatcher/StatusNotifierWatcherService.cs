using System.Reactive.Linq;
using Glimpse.Common.DBus;
using Glimpse.Common.DBus.Core;
using Glimpse.Common.Images;
using MentorLake.Redux;
using Microsoft.Extensions.Logging;
using Tmds.DBus.Protocol;

namespace Glimpse.Common.StatusNotifierWatcher;

public class StatusNotifierWatcherService(
	ILogger<StatusNotifierWatcherService> logger,
	DBusConnections dBusConnections,
	IntrospectionService introspectionService,
	ReduxStore store,
	OrgKdeStatusNotifierWatcher watcher,
	OrgFreedesktopDBus orgFreedesktopDBus)
{
	private readonly Connection _connection = dBusConnections.Session;

	public async Task InitializeAsync()
	{
		watcher.Initialize();
		watcher.RegisterStatusNotifierHostAsync("org.freedesktop.StatusNotifierWatcher-panel");
		dBusConnections.Session.AddMethodHandler(watcher);

		watcher.ItemRegistered
			.Delay(TimeSpan.FromSeconds(1))
			.Select(s => Observable.FromAsync(() => CreateTrayItemState(s.Sender, s.Service)).Take(1))
			.Concat()
			.Where(s => s != null)
			.Subscribe(s => store.Dispatch(new AddTrayItemAction(s)));

		await orgFreedesktopDBus.RequestNameAsync("org.kde.StatusNotifierWatcher", 0);
	}

	private async Task<StatusNotifierWatcherItem> CreateTrayItemState(string serviceId, string serviceName)
	{
		try
		{
			return await CreateTrayItemStateInternal(serviceId, serviceName);
		}
		catch (Exception e)
		{
			logger.LogError(e.ToString());
		}

		return null;
	}

	private async Task<StatusNotifierWatcherItem> CreateTrayItemStateInternal(string serviceId, string serviceName)
	{
		var statusNotifierItemDesc = await introspectionService.FindDBusObjectDescription(serviceId, "/", i => i == "org.kde.StatusNotifierItem");
		var statusNotifierItemProxy = new OrgKdeStatusNotifierItem(_connection, statusNotifierItemDesc.ServiceName, statusNotifierItemDesc.ObjectPath);
		var menuObjectPath = await statusNotifierItemProxy.GetMenuPropertyAsync();
		var dbusMenuDescription = await introspectionService.FindDBusObjectDescription(statusNotifierItemDesc.ServiceName, menuObjectPath, p => p == "com.canonical.dbusmenu");
		var dbusMenuProxy = new ComCanonicalDbusmenu(_connection, dbusMenuDescription.ServiceName, dbusMenuDescription.ObjectPath);
		var dbusMenuLayout = await dbusMenuProxy.GetLayoutAsync(0, -1, Array.Empty<string>());
		var itemRemovedObservable = watcher.ItemRemoved.Where(s => s == serviceId).Take(1);

		statusNotifierItemProxy.PropertyChanged
			.TakeUntil(itemRemovedObservable)
			.Subscribe(props =>
			{
				store.Dispatch(new UpdateStatusNotifierItemPropertiesAction()
				{
					Properties = CreateProperties(props),
					ServiceName = serviceId
				});
			});

		dbusMenuProxy.LayoutUpdated
			.TakeUntil(itemRemovedObservable)
			.Throttle(TimeSpan.FromMilliseconds(250))
			.Subscribe(menu =>
			{
				store.Dispatch(new UpdateMenuLayoutAction(serviceId, DbusMenuItem.From(menu.layout)));
			});

		itemRemovedObservable
			.Subscribe(_ =>
			{
				store.Dispatch(new RemoveTrayItemAction(serviceId));
			});

		return new StatusNotifierWatcherItem()
		{
			Properties = CreateProperties(await statusNotifierItemProxy.GetAllPropertiesAsync()),
			StatusNotifierItemDescription = statusNotifierItemDesc,
			DbusMenuDescription = dbusMenuDescription,
			RootMenuItem = DbusMenuItem.From(dbusMenuLayout.layout)
		};
	}

	private static StatusNotifierItemProperties CreateProperties(OrgKdeStatusNotifierItem.Properties item)
	{
		return new StatusNotifierItemProperties()
		{
			Category = item.Category,
			Id = item.Id,
			Title = item.Title,
			Status = item.Status,
			IconThemePath = item.IconThemePath,
			ItemIsMenu = item.ItemIsMenu,
			IconName = item.IconName,
			MenuPath = item.Menu.ToString(),
			IconPixmap = item.IconPixmap?.Select(i => GlimpseImageFactory.From(ImageHelper.ConvertArgbToRgba(i.Item3, i.Item1, i.Item2), 32, i.Item1, i.Item2)).ToArray(),
			OverlayIconName = item.OverlayIconName,
			OverlayIconPixmap = item.OverlayIconPixmap?.Select(i => GlimpseImageFactory.From(ImageHelper.ConvertArgbToRgba(i.Item3, i.Item1, i.Item2), 32, i.Item1, i.Item2)).ToArray(),
			AttentionIconName = item.AttentionIconName,
			AttentionIconPixmap = item.AttentionIconPixmap?.Select(i => GlimpseImageFactory.From(ImageHelper.ConvertArgbToRgba(i.Item3, i.Item1, i.Item2), 32, i.Item1, i.Item2)).ToArray(),
			AttentionMovieName = item.AttentionMovieName
		};
	}

	public async Task ActivateSystemTrayItemAsync(DbusObjectDescription desc, int x, int y)
	{
		var item = new OrgKdeStatusNotifierItem(_connection, desc.ServiceName, desc.ObjectPath);
		await item.ActivateAsync(x, y);
	}

	public async Task ContextMenuAsync(DbusObjectDescription desc, int x, int y)
	{
		var item = new OrgKdeStatusNotifierItem(_connection, desc.ServiceName, desc.ObjectPath);
		await item.ContextMenuAsync(x, y);
	}

	public async Task SecondaryActivateAsync(DbusObjectDescription desc, int x, int y)
	{
		var item = new OrgKdeStatusNotifierItem(_connection, desc.ServiceName, desc.ObjectPath);
		await item.SecondaryActivateAsync(x, y);
	}

	public async Task ActivateMenuItemAsync(DbusObjectDescription desc, int id)
	{
		var menu = new ComCanonicalDbusmenu(_connection, desc.ServiceName, desc.ObjectPath);
		await menu.EventAsync(id, "clicked", new DBusVariantItem("y", new DBusByteItem(0)), 0);
	}
}
