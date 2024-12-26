using System.Reactive.Linq;
using GLib;
using Glimpse.Common.Gtk;
using Glimpse.Common.System.Reactive;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Notifications.Components.NotificationsConfig;

public class NotificationsConfigWidget : Box
{
	public NotificationsConfigWidget(ReduxStore store) : base(Orientation.Vertical, 8)
	{
		var viewModelObs = store.Select(NotificationsConfigSelectors.ViewModel)
			.ObserveOn(GLibExt.Scheduler)
			.TakeUntilDestroyed(this)
			.Replay(1);

		var grid = new Grid()
			.Prop(w => w.RowSpacing = 8)
			.Prop(w => w.ColumnSpacing = 8);

		var rowCount = 0;

		viewModelObs
			.Select(vm => vm.KnownApplications.OrderBy(a => a.AppName))
			.UnbundleMany(a => a.AppName)
			.RemoveIndex()
			.Subscribe(appObs =>
			{
				var icon = new Image().BindViewModel(appObs.Select(a => a.AppIcon), 16);
				var label = new Label(appObs.Key.AppName) { Hexpand = true, Xalign = 0 };

				var showInPopupsSwitch = new Switch() { Halign = Align.Start, Valign = Align.Center }.BindViewModel(appObs.Select(x => x.ShowPopupBubbles));
				showInPopupsSwitch.Events().Activate.Subscribe(e => store.Dispatch(new UpdateShowInPopupsAction(appObs.Key.AppName, showInPopupsSwitch.State)));

				var showInPopupsSwitchBox = new Box(Orientation.Horizontal, 0)
					.AddClass("notifications-config__switch-container")
					.AddMany(showInPopupsSwitch);

				var showInHistorySwitch = new Switch() { Halign = Align.Start, Valign = Align.Center }.BindViewModel(appObs.Select(x => x.ShowInHistory));
				showInHistorySwitch.Events().Activate.Subscribe(e => store.Dispatch(new UpdateShowInHistoryAction(appObs.Key.AppName, showInHistorySwitch.State)));

				var showInHistorySwitchBox = new Box(Orientation.Horizontal, 0)
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

		this
			.AddClass("notifications-config__container")
			.AddMany(new Box(Orientation.Horizontal, 8)
				.AddMany(new Label("Application") { Hexpand = true, Xalign = 0 }.AddClass("notifications-config__header"))
				.AddMany(new Label("Popups") { Xalign = 0 }.AddClass("notifications-config__header"))
				.AddMany(new Label("History") { Xalign = 0 }.AddClass("notifications-config__header")))
			.AddMany(new ScrolledWindow()
				.Prop(w => w.HscrollbarPolicy = PolicyType.Automatic)
				.Prop(w => w.VscrollbarPolicy = PolicyType.Never)
				.Prop(w => w.Expand = true)
				.AddMany(grid));

		viewModelObs.Connect();
	}
}
