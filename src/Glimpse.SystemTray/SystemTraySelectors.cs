using System.Collections.Immutable;
using Glimpse.Freedesktop;
using Glimpse.Freedesktop.DBus.Interfaces;
using MentorLake.Redux.Selectors;
using Glimpse.UI.State;

namespace Glimpse.UI.Components.SystemTray;

public class SystemTraySelectors
{
	public static readonly ISelector<SystemTrayViewModel> ViewModel = SelectorFactory.Create(
		SelectorFactory.CreateFeature<SystemTrayState>(),
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
