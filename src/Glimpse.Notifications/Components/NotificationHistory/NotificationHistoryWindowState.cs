using System.Collections.Immutable;
using Glimpse.Common.Freedesktop.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.System;
using MentorLake.Redux.Selectors;

namespace Glimpse.Notifications.Components.NotificationHistory;

public record NotificationsViewModel
{
	public ImmutableList<NotificationEntryViewModel> NotificationHistory { get; set; }
}

public record NotificationEntryViewModel
{
	public Guid Id { get; set; }
	public string AppName { get; set; }
	public ImageViewModel AppIcon { get; set; }
	public string Summary { get; set; }
	public string Body { get; set; }
	public DateTime CreationDate { get; set; }
	public string DesktopEntry { get; set; }
	public ImageViewModel Image { get; set; }
}

public static class SidePaneSelectors
{
	public static readonly ISelector<NotificationsViewModel> ViewModel = SelectorFactory.Create(
		NotificationSelectors.NotificationHistory,
		DesktopFileSelectors.AllDesktopFiles,
		(history, desktopFiles) =>
		{
			return new NotificationsViewModel()
			{
				NotificationHistory = history.Notifications.Select(n =>
				{
					var appIcon = n.AppIcon;

					if (string.IsNullOrEmpty(appIcon) && !string.IsNullOrEmpty(n.DesktopEntry))
					{
						var desktopFile = desktopFiles.FirstOrDefault(f => f.Name == n.AppName);
						appIcon = desktopFile?.IconName ?? "";
					}

					return new NotificationEntryViewModel()
					{
						Id = n.Id,
						AppName = n.AppName,
						AppIcon = new ImageViewModel() { IconNameOrPath = appIcon },
						Summary = n.Summary,
						Body = n.Body,
						CreationDate = n.CreationDate,
						DesktopEntry = n.DesktopEntry,
						Image = new ImageViewModel { Image = n.Image, IconNameOrPath = n.Image == null ? n.AppIcon.Or("dialog-information-symbolic") : "" },
					};
				}).ToImmutableList()
			};
		});
}
