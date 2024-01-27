using System.Reactive.Linq;
using Glimpse.Common.Freedesktop.DesktopEntries;
using Gtk;

namespace Glimpse.Common.Gtk;

public static class ContextMenuHelper
{
	public static List<MenuItem> CreateDesktopFileActions(DesktopFile desktopFile)
	{
		var results = new List<MenuItem>();

		if (desktopFile != null && desktopFile.Actions.Count > 0)
		{
			var headerLabel = new Label("Tasks");
			headerLabel.Halign = Align.Start;
			headerLabel.StyleContext.AddClass("header-menu-item-label");

			var header = new SeparatorMenuItem();
			header.StyleContext.AddClass("header-menu-item");
			header.Add(headerLabel);

			results.Add(header);

			foreach (var action in desktopFile.Actions)
			{
				var menuItem = CreateMenuItem(action.ActionName, new ImageViewModel() { IconNameOrPath = desktopFile.IconName });
				menuItem.Data.Add("DesktopFileAction", action);
				results.Add(menuItem);
			}
		}

		return results;
	}

	public static MenuItem CreateMenuItem(string label, ImageViewModel imageViewModel)
	{
		var image = new Image();
		image.BindViewModel(Observable.Return(imageViewModel), ThemeConstants.MenuItemIconSize);

		var box = new Box(Orientation.Horizontal, 6);
		box.Add(image);
		box.Add(new Label(label));

		var menuItem = new MenuItem();
		menuItem.Add(box);

		return menuItem;
	}
}
