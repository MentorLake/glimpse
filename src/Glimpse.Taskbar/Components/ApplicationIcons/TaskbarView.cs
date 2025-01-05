using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ContextMenu;
using Glimpse.Common.Gtk.ForEach;
using Glimpse.Common.System.Reactive;
using Glimpse.Common.Xorg;
using Glimpse.Common.Xorg.State;
using Glimpse.Taskbar.Components.WindowPicker;
using Gtk;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Taskbar.Components.ApplicationIcons;

public class TaskbarView
{
	public Widget Widget { get; }

	public TaskbarView(ReduxStore store, IDisplayServer displayServer)
	{
		Widget = new Box(Orientation.Horizontal, 0);

		var viewModelSelector = store
			.Select(TaskbarViewModelSelectors.ViewModel)
			.TakeUntilDestroyed(Widget)
			.ObserveOn(GLibExt.Scheduler)
			.Replay(1)
			.AutoConnect();

		var forEachGroup = new GlimpseFlowBox<TaskbarGroupIcon>() { IsDragEnabled = true };
		Widget = forEachGroup.Widget;

		viewModelSelector
			.Select(g => g.Groups)
			.DistinctUntilChanged()
			.ObserveOn(GLibExt.Scheduler)
			.UnbundleMany(i => i.SlotRef.Id)
			.Subscribe(viewModelObservableWithIndex =>
		{
			var viewModelObservable = viewModelObservableWithIndex.Select(i => i.Item1);
			var replayLatestViewModelObservable = viewModelObservable.Replay(1).AutoConnect();
			var windowPicker = new TaskbarWindowPicker(viewModelObservable);
			var groupIcon = new TaskbarGroupIcon(viewModelObservable, windowPicker);
			var contextMenu = ContextMenuFactory.Create(groupIcon.Widget, viewModelObservable.Select(vm => vm.ContextMenuItems));

			viewModelObservable.TakeLast(1).Subscribe(_ =>
			{
				forEachGroup.RemoveItem(groupIcon);
				contextMenu.Destroy();
				windowPicker.Widget.Dispose();
				groupIcon.Widget.Destroy();
			});

			viewModelObservableWithIndex
				.Select(x => x.Item2)
				.DistinctUntilChanged()
				.Subscribe(i => forEachGroup.AddOrUpdate(groupIcon, i));

			windowPicker.Widget.ObserveEvent(w => w.Events().VisibilityNotifyEvent)
				.Subscribe(_ => windowPicker.Widget.CenterAbove(groupIcon.Widget));

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

			var cancelOpen = groupIcon.Widget.ObserveEvent(w => w.Events().LeaveNotifyEvent)
				.Merge(groupIcon.Widget.ObserveEvent(w => w.Events().Unmapped))
				.Merge(Widget.ObserveEvent(w => w.Events().Destroyed))
				.Take(1);

			groupIcon.Widget.ObserveEvent(w => w.Events().EnterNotifyEvent)
				.WithLatestFrom(replayLatestViewModelObservable)
				.Where(t => t.Second.Tasks.Count > 0)
				.Select(t => Observable.Timer(TimeSpan.FromMilliseconds(400), GLibExt.Scheduler).TakeUntil(cancelOpen).Select(_ => t.Second))
				.Switch()
				.Where(_ => !windowPicker.Widget.Visible)
				.Subscribe(t =>
				{
					store.Dispatch(new TakeScreenshotAction() { Windows = t.Tasks.Select(x => x.WindowRef).ToList() });
					windowPicker.Popup();
				});

			var cancelClose = groupIcon.Widget.ObserveEvent(w => w.Events().EnterNotifyEvent)
				.Merge(windowPicker.Widget.ObserveEvent(w => w.Events().EnterNotifyEvent));

			groupIcon.Widget.ObserveEvent(w => w.Events().LeaveNotifyEvent).Merge(windowPicker.Widget.ObserveEvent(w => w.Events().LeaveNotifyEvent))
				.Select(_ => Observable.Timer(TimeSpan.FromMilliseconds(400), GLibExt.Scheduler).TakeUntil(cancelClose))
				.Switch()
				.TakeUntilDestroyed(groupIcon.Widget)
				.TakeUntilDestroyed(windowPicker.Widget)
				.Where(_ => !windowPicker.Widget.IsPointerInside())
				.Subscribe(_ => windowPicker.ClosePopup());

			groupIcon.Widget.ObserveEvent(w => w.Events().ButtonPressEvent)
				.Subscribe(_ => windowPicker.ClosePopup());

			var primaryMouseButton = new GestureMultiPress(groupIcon.Widget);
			primaryMouseButton.Button = 1;

			var dragGesture = new GestureDrag(groupIcon.Widget);
			dragGesture.Events().DragUpdate.Subscribe(_ => primaryMouseButton.Reset());

			primaryMouseButton.Events().Released
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.Second.Tasks.Count == 0)
				.Subscribe(t => DesktopFileRunner.Run(t.Second.DesktopFile));

			primaryMouseButton.Events().Released
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.Second.Tasks.Count == 1)
				.Subscribe(t => displayServer.ToggleWindowVisibility(t.Second.Tasks.First().WindowRef));

			primaryMouseButton.Events().Released
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.Second.Tasks.Count > 1 && !windowPicker.Widget.Visible)
				.Subscribe(t =>
				{
					store.Dispatch(new TakeScreenshotAction() { Windows = t.Second.Tasks.Select(x => x.WindowRef).ToList() });
					windowPicker.Popup();
				});

			 contextMenu.ItemActivated
				.Where(i => i.Id == "Close")
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => t.Second.Tasks.ForEach(task => displayServer.CloseWindow(task.WindowRef)));

			 contextMenu.ItemActivated
				.Where(i => i.DesktopAction != null)
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => DesktopFileRunner.Run(t.First.DesktopAction));

			 contextMenu.ItemActivated
				.Where(i => i.Id == "Pin")
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => store.Dispatch(new ToggleTaskbarPinningAction(t.Second.DesktopFile.Id)));

			 contextMenu.ItemActivated
				.Where(i => i.Id == "Launch")
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => DesktopFileRunner.Run(t.Second.DesktopFile));
		});

		forEachGroup.Widget.Valign = Align.Center;
		forEachGroup.Widget.Halign = Align.Start;
		forEachGroup.Widget.Expand = false;
		forEachGroup.ItemsPerLine = 20;
		forEachGroup.ItemSpacing = 4;
		forEachGroup.Widget.AddClass("taskbar__container");

		forEachGroup.OrderingChanged
			.TakeUntilDestroyed(Widget)
			.Subscribe(ordering => store.Dispatch(new UpdateTaskbarSlotOrderingBulkAction(ordering.Select(s => s.ViewModel.SlotRef).ToImmutableList())));

		forEachGroup.DragBeginObservable
			.TakeUntilDestroyed(Widget)
			.Subscribe(icon => icon.CloseWindowPicker());
	}
}
