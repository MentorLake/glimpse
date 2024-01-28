using System.Collections.Immutable;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
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
}

public static class NotificationsConfigSelectors
{
	public static readonly ISelector<NotificationsConfigViewModel> ViewModel = SelectorFactory.Create(
		NotificationSelectors.KnownApplications,
		DesktopFileSelectors.AllDesktopFiles,
		(knownApplications, desktopFiles) =>
		{
			return new NotificationsConfigViewModel()
			{
				KnownApplications = knownApplications
					.Select(a =>
					{
						var desktopFile = desktopFiles.FirstOrDefault(d => d.Name.Equals(a.Name, StringComparison.InvariantCultureIgnoreCase))
							?? desktopFiles.FirstOrDefault(d => d.FileName.Equals(a.Name, StringComparison.InvariantCultureIgnoreCase))
							?? desktopFiles.FirstOrDefault(d => d.StartupWmClass.Equals(a.Name, StringComparison.InvariantCultureIgnoreCase))
							?? desktopFiles.FirstOrDefault(d => d.Executable.Equals(a.Name, StringComparison.InvariantCultureIgnoreCase));

						var appIcon = desktopFile?.IconName ?? "";

						return new NotificationHistoryApplicationViewModel()
						{
							AppName = a.Name,
							AppIcon = new ImageViewModel() { IconNameOrPath = appIcon }
						};
					})
					.ToImmutableList()
			};

		});
}
