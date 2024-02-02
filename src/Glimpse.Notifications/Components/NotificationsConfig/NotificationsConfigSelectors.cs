using System.Collections.Immutable;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.System;
using MentorLake.Redux.Selectors;

namespace Glimpse.Notifications.Components.NotificationsConfig;

public record NotificationsConfigViewModel
{
	public ImmutableList<NotificationHistoryApplicationViewModel> KnownApplications { get; set; }
}

public record NotificationHistoryApplicationViewModel
{
	public string AppName { get; set; }
	public ImageViewModel AppIcon { get; set; }
	public bool ShowPopupBubbles { get; set; }
	public bool ShowInHistory { get; set; }
}

public static class NotificationsConfigSelectors
{
	public static readonly ISelector<NotificationsConfigViewModel> ViewModel = SelectorFactory.Create(
		NotificationSelectors.NotificationsConfiguration,
		NotificationSelectors.KnownApplications,
		DesktopFileSelectors.AllDesktopFiles,
		(config, knownApplications, desktopFiles) =>
		{
			return new NotificationsConfigViewModel()
			{
				KnownApplications = config.Applications
					.Select(appConfig =>
					{
						var knownApp = knownApplications.FirstOrDefault(a => a.Name == appConfig.Name);

						var desktopFile = desktopFiles.FirstOrDefault(d => d.Name.Equals(appConfig.Name, StringComparison.InvariantCultureIgnoreCase))
							?? desktopFiles.FirstOrDefault(d => d.FileName.Equals(appConfig.Name, StringComparison.InvariantCultureIgnoreCase))
							?? desktopFiles.FirstOrDefault(d => d.StartupWmClass.Equals(appConfig.Name, StringComparison.InvariantCultureIgnoreCase))
							?? desktopFiles.FirstOrDefault(d => d.Executable.Equals(appConfig.Name, StringComparison.InvariantCultureIgnoreCase));

						var appIcon = desktopFile?.IconName.Or(knownApp?.Icon, "");

						return new NotificationHistoryApplicationViewModel()
						{
							AppName = appConfig.Name,
							AppIcon = new ImageViewModel() { IconNameOrPath = appIcon },
							ShowInHistory = appConfig.ShowInHistory,
							ShowPopupBubbles = appConfig.ShowPopupBubbles
						};
					})
					.ToImmutableList()
			};
		});
}
