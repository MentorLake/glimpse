using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Services.Notifications;
using Glimpse.UI.Components.Shared;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.UI.Components.NotificationsConfig;

public class NotificationsConfigWidget
{
	private readonly GtkBoxHandle _root;

	public GtkWidgetHandle Widget => _root;

	public NotificationsConfigWidget(ReduxStore store)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 8);

		var viewModelObs = store.Select(NotificationsConfigSelectors.ViewModel)
			.ObserveOn(GLibExt.Scheduler)
			.TakeUntilDestroyed(_root)
			.Replay(1);

		var grid = GtkGridHandle.New()
			.SetRowSpacing(8)
			.SetColumnSpacing(8);

		var rowCount = 0;

		viewModelObs
			.Select(vm => vm.KnownApplications.OrderBy(a => a.AppName))
			.UnbundleMany(a => a.AppName)
			.RemoveIndex()
			.Subscribe(appObs =>
			{
				var icon = GtkImageHandle.New().BindViewModel(appObs.Select(a => a.AppIcon), 16);
				var label = GtkLabelHandle.New(appObs.Key.AppName).SetHexpand(true).SetXalign(0);

				var showInPopupsSwitch = GtkSwitchHandle.New()
					.SetHalign(GtkAlign.GTK_ALIGN_START)
					.SetValign(GtkAlign.GTK_ALIGN_CENTER)
					.BindViewModel(appObs.Select(x => x.ShowPopupBubbles))
					.ObserveEvent(w => w.Signal_Activate(), e => store.Dispatch(new UpdateShowInPopupsAction(appObs.Key.AppName, e.Self.GetState())));

				var showInPopupsSwitchBox = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
					.AddClass("notifications-config__switch-container")
					.AddMany(showInPopupsSwitch);

				var showInHistorySwitch = GtkSwitchHandle.New()
					.SetHalign(GtkAlign.GTK_ALIGN_START)
					.SetValign(GtkAlign.GTK_ALIGN_CENTER)
					.BindViewModel(appObs.Select(x => x.ShowInHistory))
					.ObserveEvent(w => w.Signal_Activate(), e => store.Dispatch(new UpdateShowInHistoryAction(appObs.Key.AppName, e.Self.GetState())));

				var showInHistorySwitchBox = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
					.AddClass("notifications-config__switch-container")
					.AddMany(showInHistorySwitch);

				grid.Attach(icon, 0, rowCount, 1, 1);
				grid.Attach(label, 1, rowCount, 1, 1);
				grid.Attach(showInPopupsSwitchBox, 2, rowCount, 1, 1);
				grid.Attach(showInHistorySwitchBox, 3, rowCount, 1, 1);
				grid.ShowAll();
				rowCount++;

				appObs.TakeLast(1).Subscribe(_ =>
				{
					var rowToRemove = -1;

					for (var i = 0; i < rowCount; i++)
					{
						if (grid.GetChildAt(0, i) == icon)
						{
							rowToRemove = i;
						}
					}

					if (rowToRemove != -1)
					{
						grid.RemoveRow(rowToRemove);
					}

					rowCount--;
				});
			});

		_root
			.AddClass("notifications-config__container")
			.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 8)
				.AddMany(GtkLabelHandle.New("Application").SetHexpand(true).SetXalign(0).AddClass("notifications-config__header"))
				.AddMany(GtkLabelHandle.New("Popups").SetXalign(0).AddClass("notifications-config__header"))
				.AddMany(GtkLabelHandle.New("History").SetXalign(0).AddClass("notifications-config__header")))
			.AddMany(GtkScrolledWindowHandle.New(null, null)
				.SetPolicy(GtkPolicyType.GTK_POLICY_AUTOMATIC, GtkPolicyType.GTK_POLICY_NEVER)
				.SetHexpand(true)
				.SetVexpand(true)
				.AddMany(grid));

		viewModelObs.Connect();
	}
}
