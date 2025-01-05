using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Common.DesktopEntries;
using Glimpse.Common.Gtk;
using Glimpse.Common.Gtk.ForEach;
using Glimpse.Common.System.Reactive;
using Gtk;
using ReactiveMarbles.ObservableEvents;
using Key = Gdk.Key;

namespace Glimpse.StartMenu.Components;

public class StartMenuContent : Bin
{
	private readonly Entry _hiddenEntry;
	private readonly Subject<DesktopFileAction> _runActionSubject = new();
	private readonly Subject<DesktopFile> _appLaunch = new();
	private readonly Subject<string> _toggleTaskbarPinningSubject = new();
	private readonly Subject<string> _toggleStartMenuPinningSubject = new();
	private readonly Entry _searchEntry;
	private readonly GlimpseFlowBox<StartMenuAppIcon> _apps;
	private readonly List<(int, int)> _keyCodeRanges = new()
	{
		(48, 57),
		(65, 90),
		(97, 122)
	};

	public IObservable<string> SearchTextUpdated { get; }
	public IObservable<DesktopFile> AppLaunch => _appLaunch;
	public IObservable<DesktopFileAction> DesktopFileAction => _runActionSubject;
	public IObservable<StartMenuChips> ChipActivated { get; private set; }
	public IObservable<ImmutableList<string>> AppOrderingChanged => _apps.OrderingChanged.Select(next => next.Select(a => a.Id).ToImmutableList());
	public IObservable<string> ToggleTaskbarPinning => _toggleTaskbarPinningSubject;
	public IObservable<string> ToggleStartMenuPinning => _toggleStartMenuPinningSubject;

	public StartMenuContent(IObservable<StartMenuViewModel> viewModelObservable, StartMenuActionBar actionBar)
	{
		_hiddenEntry = new Entry();
		_hiddenEntry.IsEditable = false;

		_searchEntry = new Entry("");
		_searchEntry.IsEditable = true;
		_searchEntry.Valign = Align.Center;
		_searchEntry.Halign = Align.Center;
		_searchEntry.PrimaryIconName = "edit-find-symbolic";
		_searchEntry.PlaceholderText = "Search all applications";
		_searchEntry.AddClass("start-menu__search-input");

		SearchTextUpdated = Observable.Return("")
			.Merge(_searchEntry.ObserveEvent(w => w.Events().TextInserted).Select(_ => _searchEntry.Text))
			.Merge(_searchEntry.ObserveEvent(w => w.Events().TextDeleted).Select(_ => _searchEntry.Text))
			.TakeUntilDestroyed(this)
			.Throttle(TimeSpan.FromMilliseconds(50), GLibExt.Scheduler)
			.DistinctUntilChanged();

		var chipsObs = viewModelObservable.Select(vm => vm.Chips).DistinctUntilChanged();
		var pinnedChip = Chip.Create("Pinned", chipsObs.Select(c => c[StartMenuChips.Pinned]));
		var allAppsChip = Chip.Create("All Apps", chipsObs.Select(c => c[StartMenuChips.AllApps]));
		var searchResultsChip = Chip.Create("Search results", chipsObs.Select(c => c[StartMenuChips.SearchResults]));

		ChipActivated = pinnedChip.ObserveEvent(w => w.Events().ButtonReleaseEvent).Select(_ => StartMenuChips.Pinned)
			.Merge(allAppsChip.ObserveEvent(w => w.Events().ButtonReleaseEvent).Select(_ => StartMenuChips.AllApps))
			.Merge(searchResultsChip.ObserveEvent(w => w.Events().ButtonReleaseEvent).Select(_ => StartMenuChips.SearchResults));

		var chipBox = new Box(Orientation.Horizontal, 4);
		chipBox.Halign = Align.Start;
		chipBox.Add(pinnedChip);
		chipBox.Add(allAppsChip);
		chipBox.Add(searchResultsChip);
		chipBox.AddClass("start-menu__chips");

		_apps = new GlimpseFlowBox<StartMenuAppIcon>();
		_apps.ItemSpacing = 0;
		_apps.ItemsPerLine = 6;
		_apps.Widget.Valign = Align.Start;
		_apps.Widget.Halign = Align.Start;
		_apps.Widget.AddClass("start-menu__apps");
		_apps.FilterFunc = c => c.ViewModel.IsVisible;

		viewModelObservable
			.TakeUntilDestroyed(this)
			.Select(vm => vm.DisableDragAndDrop)
			.DistinctUntilChanged()
			.Subscribe(x => _apps.IsDragEnabled = !x);

		viewModelObservable.Select(vm => vm.AllApps).DistinctUntilChanged().UnbundleMany(i => i.DesktopFile.FilePath).RemoveIndex().Subscribe(itemObservable =>
		{
			var appIcon = new StartMenuAppIcon(itemObservable.Key.DesktopFile.Id, itemObservable);
			itemObservable.Select(vm => vm.IsVisible).DistinctUntilChanged().ObserveOn(GLibExt.Scheduler).Subscribe(_ => _apps.InvalidateFilter());
			itemObservable.Select(vm => vm.Index).DistinctUntilChanged().ObserveOn(GLibExt.Scheduler).Subscribe(i => _apps.AddOrUpdate(appIcon, i));
			itemObservable.TakeLast(1).ObserveOn(GLibExt.Scheduler).Subscribe(_ => appIcon.Widget.Destroy());

			appIcon.ContextMenuItemActivated
				.TakeUntilDestroyed(appIcon.Widget)
				.Where(i => i.Id == StartMenuAppContextMenuItem.ToggleTaskbarAppId)
				.Subscribe(i => _toggleTaskbarPinningSubject.OnNext(i.DesktopFilePath));

			appIcon.ContextMenuItemActivated
				.TakeUntilDestroyed(appIcon.Widget)
				.Where(i => i.Id == StartMenuAppContextMenuItem.ToggleStartMenuAppId)
				.Subscribe(i => _toggleStartMenuPinningSubject.OnNext(i.DesktopFilePath));

			appIcon.ContextMenuItemActivated
				.TakeUntilDestroyed(appIcon.Widget)
				.Where(i => i.Id != StartMenuAppContextMenuItem.ToggleTaskbarAppId && i.Id != StartMenuAppContextMenuItem.ToggleStartMenuAppId)
				.Subscribe(i => _runActionSubject.OnNext(i.DesktopAction));
		});

		_apps.ItemActivated.TakeUntilDestroyed(_apps.Widget)
			.Subscribe(i => _appLaunch.OnNext(i.ViewModel.DesktopFile));

		_searchEntry.ObserveEvent(w => w.Events().KeyReleaseEvent)
			.Where(e => e.Event.Key == Key.Return || e.Event.Key == Key.KP_Enter)
			.WithLatestFrom(viewModelObservable.Select(vm => vm.AllApps.Where(a => a.IsVisible)).DistinctUntilChanged())
			.Where(t => t.Second.Any())
			.Subscribe(t => _appLaunch.OnNext(t.Second.FirstOrDefault().DesktopFile));

		var pinnedAppsScrolledWindow = new ScrolledWindow();
		pinnedAppsScrolledWindow.Vexpand = true;
		pinnedAppsScrolledWindow.Add(_apps.Widget);
		pinnedAppsScrolledWindow.AddClass("start-menu__apps-scroll-window");

		var layout = new Grid();
		layout.Expand = true;
		layout.ColumnHomogeneous = true;
		layout.Attach(_hiddenEntry, 1, 0, 1, 1);
		layout.Attach(_searchEntry, 1, 0, 6, 1);
		layout.Attach(chipBox, 1, 1, 6, 1);
		layout.Attach(pinnedAppsScrolledWindow, 1, 2, 6, 8);
		layout.Attach(actionBar, 1, 10, 6, 1);
		layout.StyleContext.AddClass("start-menu__window");

		Add(layout);
		ShowAll();
		_hiddenEntry.Hide();
	}

	public void HandleWindowShown()
	{
		_searchEntry.Text = "";
		_hiddenEntry.GrabFocus();
	}

	public bool HandleKeyPress(uint keyValue)
	{
		if (!_searchEntry.HasFocus && _keyCodeRanges.Any(r => keyValue >= r.Item1 && keyValue <= r.Item2))
		{
			_searchEntry.GrabFocusWithoutSelecting();
		}
		else if (_hiddenEntry.HasFocus)
		{
			_searchEntry.GrabFocusWithoutSelecting();
			return true;
		}

		return false;
	}
}
