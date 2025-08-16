using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Services.Taskbar;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ForEach;
using Glimpse.UI.Components.WindowPicker;
using MentorLake.Gtk;
using MentorLake.Redux;

namespace Glimpse.UI.Components.ApplicationIcons;

public class TaskbarView
{
	public GtkWidgetHandle Widget { get; }

	public TaskbarView(ReduxStore store, IDisplayServer displayServer)
	{
		Widget = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);

		var viewModelSelector = store
			.Select(TaskbarViewModelSelectors.ViewModel)
			.TakeUntilDestroyed(Widget)
			.ObserveOn(GLibExt.Scheduler)
			.Replay(1)
			.AutoConnect();

		var forEachGroup = new GlimpseFlowBox<TaskbarGroupIcon>() { IsDragEnabled = true };
		Widget = forEachGroup.Widget;

		viewModelSelector
			.TakeUntilDestroyed(forEachGroup.Widget)
			.Select(g => g.Groups)
			.DistinctUntilChanged()
			.ObserveOn(GLibExt.Scheduler)
			.UnbundleMany(i => i.SlotRef.Id)
			.Subscribe(viewModelObservableWithIndex =>
		{
			var viewModelObservable = viewModelObservableWithIndex.Select(i => i.Item1);
			var replayLatestViewModelObservable = viewModelObservable.Replay(1).AutoConnect();
			var windowPicker = new TaskbarWindowPicker(viewModelObservable);
			var appIconViewModelObs = viewModelObservable.Select(vm => new AppIconViewModel<SlotContextMenuItemViewModel>() { IconInfo = vm.Icon, ContextMenuItems = vm.ContextMenuItems });
			var groupIcon = new TaskbarGroupIcon(windowPicker, appIconViewModelObs);

			viewModelObservable.Select(vm => vm.Tasks.Count).DistinctUntilChanged().Subscribe(c => groupIcon.UpdateTaskCount(c));
			viewModelObservable.Select(vm => vm.DemandsAttention).DistinctUntilChanged().Subscribe(c => groupIcon.UpdateDemandsAttention(c));
			viewModelObservable.Select(vm => vm.SlotRef).DistinctUntilChanged().Subscribe(s => groupIcon.UpdateSlotRef(s));

			viewModelObservable.TakeLast(1).TakeUntilDestroyed(forEachGroup.Widget).Subscribe(_ =>
			{
				forEachGroup.RemoveItem(groupIcon);
				windowPicker.Widget.Destroy();
			});

			viewModelObservableWithIndex
				.Select(x => x.Item2)
				.DistinctUntilChanged()
				.Subscribe(i => forEachGroup.AddOrUpdate(groupIcon, i));

			windowPicker.Widget.ObserveEvent(w => w.Signal_VisibilityNotifyEvent())
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

			var cancelOpen = groupIcon.Widget.ObserveEvent(w => w.Signal_LeaveNotifyEvent().Select(_ => true))
				.Merge(groupIcon.Widget.ObserveEvent(w => w.Signal_Unmap()).Select(_ => true))
				.Merge(Widget.ObserveEvent(w => w.Signal_Destroy().Select(_ => true)))
				.Take(1);

			groupIcon.Widget.ObserveEvent(w => w.Signal_EnterNotifyEvent())
				.WithLatestFrom(replayLatestViewModelObservable)
				.Where(t => t.Second.Tasks.Count > 0)
				.Select(t => Observable.Timer(TimeSpan.FromMilliseconds(400), GLibExt.Scheduler).TakeUntil(cancelOpen).Select(_ => t.Second))
				.Switch()
				.Where(_ => !windowPicker.Widget.GetVisible())
				.Subscribe(t =>
				{
					store.Dispatch(new TakeScreenshotAction() { Windows = t.Tasks.Select(x => x.WindowRef).ToList() });
					windowPicker.Popup();
				});

			var cancelClose = groupIcon.Widget.ObserveEvent(w => w.Signal_EnterNotifyEvent())
				.Merge(windowPicker.Widget.ObserveEvent(w => w.Signal_EnterNotifyEvent()));

			groupIcon.Widget.ObserveEvent(w => w.Signal_LeaveNotifyEvent()).Merge(windowPicker.Widget.ObserveEvent(w => w.Signal_LeaveNotifyEvent()))
				.Select(_ => Observable.Timer(TimeSpan.FromMilliseconds(400), GLibExt.Scheduler).TakeUntil(cancelClose))
				.Switch()
				.TakeUntilDestroyed(groupIcon.Widget)
				.TakeUntilDestroyed(windowPicker.Widget)
				.Where(_ => !windowPicker.Widget.IsPointerInside())
				.Subscribe(_ => windowPicker.ClosePopup());

			groupIcon.Widget.ObserveEvent(w => w.Signal_ButtonPressEvent())
				.Subscribe(_ => windowPicker.ClosePopup());

			var primaryMouseButton = GtkGestureMultiPressHandle.New(groupIcon.Widget);
			primaryMouseButton.SetButton(1);

			forEachGroup.DragBeginObservable
				.TakeUntilDestroyed(groupIcon.Widget)
				.Subscribe(_ => primaryMouseButton.Reset());

			primaryMouseButton.Signal_Released()
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.Second.Tasks.Count == 0)
				.Subscribe(t => DesktopFileRunner.Run(t.Second.DesktopFile));

			primaryMouseButton.Signal_Released()
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.Second.Tasks.Count == 1)
				.Subscribe(t => displayServer.ToggleWindowVisibility(t.Second.Tasks.First().WindowRef));

			primaryMouseButton.Signal_Released()
				.WithLatestFrom(viewModelObservable)
				.Where(t => t.Second.Tasks.Count > 1 && !windowPicker.Widget.GetVisible())
				.Subscribe(t =>
				{
					store.Dispatch(new TakeScreenshotAction() { Windows = t.Second.Tasks.Select(x => x.WindowRef).ToList() });
					windowPicker.Popup();
				});

			groupIcon.ContextMenuItemActivated
				.Where(i => i.Id == "Close")
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => t.Second.Tasks.ForEach(task => displayServer.CloseWindow(task.WindowRef)));

			groupIcon.ContextMenuItemActivated
				.Where(i => i.DesktopAction != null)
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => DesktopFileRunner.Run(t.First.DesktopAction));

			groupIcon.ContextMenuItemActivated
				.Where(i => i.Id == "Pin")
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => store.Dispatch(new ToggleTaskbarPinningAction(t.Second.DesktopFile.Id)));

			groupIcon.ContextMenuItemActivated
				.Where(i => i.Id == "Launch")
				.WithLatestFrom(viewModelObservable)
				.Subscribe(t => DesktopFileRunner.Run(t.Second.DesktopFile));
		});

		forEachGroup.Widget.SetValign(GtkAlign.GTK_ALIGN_CENTER);
		forEachGroup.Widget.SetHalign(GtkAlign.GTK_ALIGN_START);
		forEachGroup.ItemsPerLine = 20;
		forEachGroup.ItemSpacing = 4;
		forEachGroup.Widget.AddClass("taskbar__container");

		forEachGroup.OrderingChanged
			.TakeUntilDestroyed(Widget)
			.Subscribe(ordering => store.Dispatch(new UpdateTaskbarSlotOrderingBulkAction(ordering.Select(s => s.SlotRef).ToImmutableList())));

		forEachGroup.DragBeginObservable
			.TakeUntilDestroyed(Widget)
			.Subscribe(icon => icon.CloseWindowPicker());
	}
}
