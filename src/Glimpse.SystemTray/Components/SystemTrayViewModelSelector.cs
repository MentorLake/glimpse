using System.Collections.Immutable;
using Glimpse.Common.Gtk;
using Glimpse.Common.StatusNotifierWatcher;
using MentorLake.Redux.Selectors;

namespace Glimpse.SystemTray.Components;

public class SystemTrayViewModelSelector
{
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
						RootMenuItem = x.RootMenuItem
					};
				}).ToImmutableList()
			};
		});
}
