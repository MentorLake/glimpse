using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using GLib;
using Glimpse.Common.Freedesktop.DesktopEntries;
using Glimpse.Common.Freedesktop.Xorg;
using Glimpse.Common.Freedesktop.Xorg.State;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ForEach;
using Glimpse.Taskbar.Components.WindowPicker;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

public class TaskbarView : Box
{
	public TaskbarView(ReduxStore store, IDisplayServer displayServer)
	{
		var viewModelSelector = store
			.Select(TaskbarViewModelSelectors.ViewModel)
			.TakeUntilDestroyed(this)
			.ObserveOn(new GLibSynchronizationContext())
			.Replay(1);

		var forEachGroup = ForEachExtensions.Create(viewModelSelector.Select(g => g.Groups).DistinctUntilChanged(), i => i.SlotRef, viewModelObservable =>
		{
			var replayLatestViewModelObservable = viewModelObservable.Replay(1);
			var contextMenu = new TaskbarGroupContextMenu(viewModelObservable.Select(vm => vm.ContextMenu).DistinctUntilChanged());
			var windowPicker = new TaskbarWindowPicker(viewModelObservable);
			var groupIcon = new TaskbarGroupIcon(viewModelObservable, windowPicker);

			viewModelObservable.TakeLast(1).Subscribe(_ =>
			{
				contextMenu.Destroy();
				windowPicker.Dispose();
			});

			windowPicker.ObserveEvent(w => w.Events().VisibilityNotifyEvent)
				.Subscribe(_ => windowPicker.CenterAbove(groupIcon));

			windowPicker.PreviewWindowClicked
				.Subscribe(windowId =>
				{
					windowPicker.ClosePopup();
					displayServer.MakeWindowVisible(windowId);
				});

			windowPicker.CloseWindow
				.WithLatestFrom(viewModelObservable.Select(vm => vm.Tasks.Count).DistinctUntilChanged())
				.Where(t => t.Second == 1)
				.Subscribe(_ => windowPicker.ClosePopup());

			windowPicker.CloseWindow
				.Subscribe(displayServer.CloseWindow);

			var cancelOpen = groupIcon.ObserveEvent(w => w.Events().LeaveNotifyEvent)
				.Merge(groupIcon.ObserveEvent(w => w.Events().Unmapped))
				.Merge(this.ObserveEvent(w => w.Events().Destroyed))
				.Take(1);

			groupIcon.ObserveEvent(w => w.Events().EnterNotifyEvent)
				.WithLatestFrom(replayLatestViewModelObservable)
				.Where(t => t.Second.Tasks.Count > 0)
				.Select(t => Observable.Timer(TimeSpan.FromMilliseconds(400), new SynchronizationContextScheduler(new GLibSynchronizationContext())).TakeUntil(cancelOpen).Select(_ => t.Second))
				.Switch()
				.Where(_ => !windowPicker.Visible)
				.Subscribe(t =>
				{
					store.Dispatch(new TakeScreenshotAction() { Windows = t.Tasks.Select(x => x.WindowRef).ToList() });
					windowPicker.Popup();
				});

			var cancelClose = groupIcon.ObserveEvent(w => w.Events().EnterNotifyEvent)
				.Merge(windowPicker.ObserveEvent(w => w.Events().EnterNotifyEvent));

			groupIcon.ObserveEvent(w => w.Events().LeaveNotifyEvent).Merge(windowPicker.ObserveEvent(w => w.Events().LeaveNotifyEvent))
				.Select(_ => Observable.Timer(TimeSpan.FromMilliseconds(400), new SynchronizationContextScheduler(new GLibSynchronizationContext())).TakeUntil(cancelClose))
				.Switch()
				.TakeUntilDestroyed(groupIcon)
				.TakeUntilDestroyed(windowPicker)
				.Where(_ => !windowPicker.IsPointerInside())
				.Subscribe(_ => windowPicker.ClosePopup());

			groupIcon.CreateContextMenuObservable()
				.Subscribe(_ => contextMenu.Popup());

			groupIcon.ObserveEvent(w => w.Events().ButtonPressEvent)
				.Subscribe(_ => windowPicker.ClosePopup());

			groupIcon.ObserveButtonRelease()
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.First.Event.Button == 1 && t.Second.Tasks.Count == 0)
				.Subscribe(t => DesktopFileRunner.Run(t.Second.DesktopFile));

			groupIcon.ObserveButtonRelease()
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.First.Event.Button == 1 && t.Second.Tasks.Count == 1)
				.Subscribe(t => displayServer.ToggleWindowVisibility(t.Second.Tasks.First().WindowRef));

			 groupIcon.ObserveButtonRelease()
			 	.WithLatestFrom(viewModelObservable)
			 	.Where(t => t.First.Event.Button == 1 && t.Second.Tasks.Count > 1 && !windowPicker.Visible)
			 	.Subscribe(t =>
			 	{
			 		store.Dispatch(new TakeScreenshotAction() { Windows = t.Second.Tasks.Select(x => x.WindowRef).ToList() });
			 		windowPicker.Popup();
			 	});

			 contextMenu.WindowAction
			 	.WithLatestFrom(viewModelObservable)
			 	.Subscribe(t => t.Second.Tasks.ForEach(task => displayServer.CloseWindow(task.WindowRef)));

			 contextMenu.DesktopFileAction
			 	.WithLatestFrom(viewModelObservable)
			 	.Subscribe(t => DesktopFileRunner.Run(t.First));

			 contextMenu.Pin
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => store.Dispatch(new ToggleTaskbarPinningAction(t.Second.DesktopFile.Id)));

			 contextMenu.Launch
			 	.WithLatestFrom(viewModelObservable)
			 	.Subscribe(t => DesktopFileRunner.Run(t.Second.DesktopFile));

			replayLatestViewModelObservable.Connect();
			return groupIcon;
		});

		forEachGroup.ColumnSpacing = 0;
		forEachGroup.MaxChildrenPerLine = 100;
		forEachGroup.MinChildrenPerLine = 100;
		forEachGroup.RowSpacing = 0;
		forEachGroup.Orientation = Orientation.Horizontal;
		forEachGroup.Homogeneous = false;
		forEachGroup.Valign = Align.Center;
		forEachGroup.Halign = Align.Start;
		forEachGroup.SelectionMode = SelectionMode.None;
		forEachGroup.Expand = false;
		forEachGroup.AddClass("taskbar__container");
		forEachGroup.OrderingChanged
			.TakeUntilDestroyed(this)
			.Subscribe(ordering => store.Dispatch(new UpdateTaskbarSlotOrderingBulkAction(ordering.Select(s => s.SlotRef).ToImmutableList())));
		forEachGroup.DragBeginObservable.TakeUntilDestroyed(this).Subscribe(icon => icon.CloseWindowPicker());

		Add(forEachGroup);

		viewModelSelector.Connect();
	}
}
