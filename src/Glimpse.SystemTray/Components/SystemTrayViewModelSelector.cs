using System.Collections.Immutable;
using Glimpse.Common.DBus;
using Glimpse.Common.Gtk;
using Glimpse.Common.Images;
using Glimpse.Common.StatusNotifierWatcher;
using MentorLake.Redux.Selectors;

namespace Glimpse.SystemTray.Components;

public class SystemTrayViewModelSelector
{
	private static SystemTrayContextMenuItemViewModel CreateContextMenuItemViewModel(DbusMenuItem dbusMenuItem)
	{
		if (dbusMenuItem.Type == "separator")
		{
			return new SystemTrayContextMenuItemViewModel() { DisplayText = "separator" };
		}

		var image = dbusMenuItem.IconData == null ? null : GlimpseImageFactory.From(dbusMenuItem.IconData);

		return new SystemTrayContextMenuItemViewModel()
		{
			DBusId = dbusMenuItem.Id,
			DisplayText = dbusMenuItem.Label,
			Icon = new ImageViewModel() { IconNameOrPath = dbusMenuItem.IconName ?? "", Image = image },
			Children = dbusMenuItem.Children.Select(CreateContextMenuItemViewModel).ToImmutableList()
		};
	}

	public static readonly ISelector<SystemTrayViewModel> ViewModel = SelectorFactory.Create(
		StatusNotifierWatcherSelectors.StatusNotifierWatcherState,
		state =>
		{
			return new SystemTrayViewModel()
			{
				Items = state.Items.Values.Select(x =>
				{
					var iconName = "";

					if (!string.IsNullOrEmpty(x.Properties.IconThemePath))
					{
						iconName = Path.Join(x.Properties.IconThemePath, x.Properties.IconName) + ".png";
					}
					else if (!string.IsNullOrEmpty(x.Properties.IconName))
					{
						iconName = x.Properties.IconName;
					}

					return new SystemTrayItemViewModel()
					{
						Id = x.Properties.Id,
						Icon = new ImageViewModel() { IconNameOrPath = iconName, Image = x.Properties.IconPixmap?.MaxBy(i => i.Width * i.Height) },
						Tooltip = x.Properties.Title,
						CanActivate = x.StatusNotifierItemDescription.InterfaceHasMethod(OrgKdeStatusNotifierItem.Interface, "Activate"),
						StatusNotifierItemDescription = x.StatusNotifierItemDescription,
						DbusMenuDescription = x.DbusMenuDescription,
						ContextMenuItems = x.RootMenuItem.Children.Select(CreateContextMenuItemViewModel).ToImmutableList()
					};
				}).ToImmutableList()
			};
		});
}
