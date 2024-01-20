using Gdk;
using Glimpse.Freedesktop.DBus;
using Gtk;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;

namespace Glimpse.UI.Components.SystemTray;

public static class DbusContextMenuHelpers
{
	public static DbusMenuItem GetDbusMenuItem(this MenuItem menuItem)
	{
		return menuItem.Data["DbusMenuItem"] as DbusMenuItem;
	}
	public static LinkedList<MenuItem> GetAllMenuItems(Menu rootMenu)
	{
		var result = new LinkedList<MenuItem>();
		var queue = new Queue<Menu>();
		queue.Enqueue(rootMenu);

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			current.Children.OfType<MenuItem>().ToList().ForEach(i => result.AddLast(i));
			current.Children.OfType<Menu>().ToList().ForEach(i => queue.Enqueue(i));
		}

		return result;
	}
	public static void PopulateMenu(Menu rootMenu, DbusMenuItem rootItem)
	{
		var items = new List<DbusMenuItem>();

		foreach (var element in rootItem.Children)
		{
			if (items.Count > 0 && items.Last().Type == "separator" && element.Type == "separator") continue;
			items.Add(element);
		}

		if (items.Count > 0 && items.Last().Type == "separator") items = items.SkipLast(1).ToList();
		foreach (var item in items) PopulateMenuInternal(rootMenu, item);
		rootMenu.ShowAll();
	}

	private static void PopulateMenuInternal(Menu parentMenu, DbusMenuItem dbusMenuItem)
	{
		if (dbusMenuItem.Type == "separator")
		{
			parentMenu.Add(new SeparatorMenuItem());
		}
		else
		{
			var menuItem = CreateMenuItem(dbusMenuItem);
			parentMenu.Add(menuItem);

			if (dbusMenuItem.Children.Any())
			{
				var childMenu = new Menu();
				menuItem.Submenu = childMenu;
				foreach (var childItem in dbusMenuItem.Children) PopulateMenuInternal(childMenu, childItem);
			}
		}
	}

	private static MenuItem CreateMenuItem(DbusMenuItem item)
	{
		var box = new Box(Orientation.Horizontal, 10);

		if (item.IconData != null)
		{
			using var memoryStream = new MemoryStream(item.IconData);
			box.PackStart(new Image(memoryStream), false, false, 0);
		}
		else if (!string.IsNullOrEmpty(item.IconName))
		{
			var image = new Image();
			image.SetFromIconName(item.IconName, IconSize.Menu);
			box.PackStart(image, false, false, 0);
		}

		box.PackStart(new Label(item.Label), false, false, 0);

		var menuItem = new MenuItem();
		menuItem.Data.Add("DbusMenuItem", item);
		menuItem.Add(box);
		return menuItem;
	}
}
