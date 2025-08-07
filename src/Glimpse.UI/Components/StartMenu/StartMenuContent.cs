using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.ForEach;
using MentorLake.Gdk;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.StartMenu;

public class StartMenuContent
{
	private readonly GtkBoxHandle _root;
	private readonly GtkEntryHandle _hiddenEntry;
	private readonly Subject<DesktopFileAction> _runActionSubject = new();
	private readonly Subject<DesktopFile> _appLaunch = new();
	private readonly Subject<string> _toggleTaskbarPinningSubject = new();
	private readonly Subject<string> _toggleStartMenuPinningSubject = new();
	private readonly GtkEntryHandle _searchEntry;
	private readonly Dictionary<string, (AppIcon<StartMenuAppContextMenuItem> All, AppIcon<StartMenuAppContextMenuItem> Pinned, AppIcon<StartMenuAppContextMenuItem> Search)> _appIcons = new();
	private readonly Dictionary<string, (StartMenuAppViewModel ViewModel, int SearchIndex)> _appIconViewModels = new();
	private readonly GlimpseFlowBox<AppIcon<StartMenuAppContextMenuItem>> _pinnedApps;
	private readonly GlimpseFlowBox<AppIcon<StartMenuAppContextMenuItem>> _searchApps;
	private readonly GlimpseFlowBox<AppIcon<StartMenuAppContextMenuItem>> _allApps;
	private readonly List<(int, int)> _keyCodeRanges = new()
	{
		(48, 57),
		(65, 90),
		(97, 122)
	};
	private readonly ChipGroup _chipGroup;

	public GtkWidgetHandle Widget => _root;
	public IObservable<string> SearchTextUpdated { get; }
	public IObservable<DesktopFile> AppLaunch => _appLaunch;
	public IObservable<DesktopFileAction> DesktopFileAction => _runActionSubject;
	public IObservable<ImmutableList<string>> AppOrderingChanged => _pinnedApps.OrderingChanged.Select(next => next.Select(a => a.Id).ToImmutableList());
	public IObservable<string> ToggleTaskbarPinning => _toggleTaskbarPinningSubject;
	public IObservable<string> ToggleStartMenuPinning => _toggleStartMenuPinningSubject;

	public StartMenuContent(StartMenuActionBar actionBar)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		_hiddenEntry = GtkEntryHandle.New()
			.SetEditable(false);

		_searchEntry = GtkEntryHandle.New()
			.AddClass("start-menu__search-input")
			.SetEditable(true)
			.SetValign(GtkAlign.GTK_ALIGN_CENTER)
			.SetHalign(GtkAlign.GTK_ALIGN_CENTER)
			.SetIconFromIconName(GtkEntryIconPosition.GTK_ENTRY_ICON_PRIMARY, "edit-find-symbolic")
			.SetPlaceholderText("Search all applications");

		SearchTextUpdated = Observable.Return("")
			.Merge(_searchEntry.ObserveEvent(w => w.Signal_InsertText()).Select(_ => _searchEntry.GetText()))
			.Merge(_searchEntry.ObserveEvent(w => w.Signal_DeleteText()).Select(_ => _searchEntry.GetText()))
			.TakeUntilDestroyed(_root)
			.Throttle(TimeSpan.FromMilliseconds(50), GLibExt.Scheduler)
			.DistinctUntilChanged();

		_pinnedApps = CreatePage();
		_pinnedApps.IsDragEnabled = true;

		_allApps = CreatePage();
		_allApps.IsDragEnabled = false;

		_searchApps = CreatePage();
		_searchApps.IsDragEnabled = false;

		var pages = new Dictionary<string, GtkScrolledWindowHandle>()
		{
			["pinned"] = GtkScrolledWindowHandle.New(null, null)
				.SetVexpand(true)
				.Add(_pinnedApps.Widget)
				.AddClass("start-menu__apps-scroll-window")
				.ShowAll(),
			["all-apps"] = GtkScrolledWindowHandle.New(null, null)
				.SetVexpand(true)
				.Add(_allApps.Widget)
				.AddClass("start-menu__apps-scroll-window")
				.ShowAll(),
			["search"] = GtkScrolledWindowHandle.New(null, null)
				.SetVexpand(true)
				.Add(_searchApps.Widget)
				.AddClass("start-menu__apps-scroll-window")
				.ShowAll()
		};

		_chipGroup = new ChipGroup(4);
		_chipGroup.AddChip("pinned", "Pinned");
		_chipGroup.AddChip("all-apps", "All Apps");
		_chipGroup.AddChip("search", "Search results");
		_chipGroup.Widget.SetHalign(GtkAlign.GTK_ALIGN_START);
		_chipGroup.Widget.AddClass("start-menu__chips");

		var layout = GtkGridHandle.New()
			.SetColumnHomogeneous(true)
			.Attach(_hiddenEntry, 1, 0, 1, 1)
			.Attach(_searchEntry, 1, 0, 6, 1)
			.Attach(_chipGroup.Widget, 1, 1, 6, 1)
			.Attach(pages["pinned"], 1, 2, 6, 8)
			.Attach(pages["all-apps"], 1, 2, 6, 8)
			.Attach(pages["search"], 1, 2, 6, 8)
			.Attach(actionBar.Widget, 1, 10, 6, 1)
			.AddClass("start-menu__content");

		_chipGroup.ChipSelectionUpdated.Select(chips => chips.First()).Subscribe(c =>
		{
			foreach (var p in pages) p.Value.Hide();
			pages[c].Show();
		});

		_pinnedApps.ItemActivated.TakeUntilDestroyed(_pinnedApps.Widget)
			.SubscribeDebug(i => _appLaunch.OnNext(_appIconViewModels[i.Id].ViewModel.DesktopFile));

		_allApps.ItemActivated.TakeUntilDestroyed(_allApps.Widget)
			.SubscribeDebug(i => _appLaunch.OnNext(_appIconViewModels[i.Id].ViewModel.DesktopFile));

		_searchApps.ItemActivated.TakeUntilDestroyed(_searchApps.Widget)
			.SubscribeDebug(i => _appLaunch.OnNext(_appIconViewModels[i.Id].ViewModel.DesktopFile));

		_searchEntry.ObserveEvent(w => w.Signal_InsertText())
			.Subscribe(_ => _chipGroup.Select("search"));

		_searchEntry.ObserveEvent(w => w.Signal_DeleteText())
			.Where(_ => string.IsNullOrEmpty(_searchEntry.GetText()))
			.SubscribeDebug(e =>
			{
				_chipGroup.Select("pinned");
				_chipGroup.HideChip("search");
			});

		_searchEntry.ObserveEvent(w => w.Signal_KeyReleaseEvent()
			.Select(a => a.Event.Dereference())
			.Where(e => e.keyval == GdkConstants.KEY_Return || e.keyval == GdkConstants.KEY_KP_Enter)
			.Select(_ => _appIconViewModels.FirstOrDefault(vm => vm.Value.SearchIndex == 0).Key)
			.Where(k => !string.IsNullOrEmpty(k)))
			.SubscribeDebug(k => _appLaunch.OnNext(_appIconViewModels[k].ViewModel.DesktopFile));

		_root.Add(layout);
		_root.ShowAll();
		_hiddenEntry.Hide();
	}

	private GlimpseFlowBox<AppIcon<StartMenuAppContextMenuItem>> CreatePage()
	{
		var page = new GlimpseFlowBox<AppIcon<StartMenuAppContextMenuItem>>();
		page.ItemSpacing = 0;
		page.ItemsPerLine = 6;
		page.Widget.SetValign(GtkAlign.GTK_ALIGN_START);
		page.Widget.SetHalign(GtkAlign.GTK_ALIGN_START);
		page.Widget.AddClass("start-menu__apps");
		return page;
	}

	private AppIcon<StartMenuAppContextMenuItem> CreateAppIcon(string applicationId, IObservable<AppIconViewModel<StartMenuAppContextMenuItem>> viewModelObs)
	{
		var appIcon = new AppIcon<StartMenuAppContextMenuItem>(applicationId, 36, viewModelObs);
		appIcon.Widget.AddClass("start-menu__app-icon-container");

		appIcon.ContextMenuItemActivated
			.TakeUntilDestroyed(appIcon.Widget)
			.Where(i => i.Id == StartMenuAppContextMenuItem.ToggleTaskbarAppId)
			.SubscribeDebug(i => _toggleTaskbarPinningSubject.OnNext(i.DesktopFile.FilePath));

		appIcon.ContextMenuItemActivated
			.TakeUntilDestroyed(appIcon.Widget)
			.Where(i => i.Id == StartMenuAppContextMenuItem.ToggleStartMenuAppId)
			.SubscribeDebug(i => _toggleStartMenuPinningSubject.OnNext(i.DesktopFile.FilePath));

		appIcon.ContextMenuItemActivated
			.TakeUntilDestroyed(appIcon.Widget)
			.Where(i => i.Id != StartMenuAppContextMenuItem.ToggleTaskbarAppId && i.Id != StartMenuAppContextMenuItem.ToggleStartMenuAppId)
			.SubscribeDebug(i => _runActionSubject.OnNext(i.DesktopAction));

		return appIcon;
	}

	public void RemoveApplication(StartMenuAppViewModel app)
	{
		var appIcons = _appIcons[app.DesktopFile.Id];
		_appIcons.Remove(app.DesktopFile.Id);
		_allApps.RemoveItem(appIcons.All);
		_pinnedApps.RemoveItem(appIcons.Pinned);
		_searchApps.RemoveItem(appIcons.Search);
	}

	public void AddApplication(string applicationId, IObservable<StartMenuAppViewModel> viewModelObs, IObservable<int> searchIndexObs)
	{
		var appIconViewModel = viewModelObs
			.Select(vm => new AppIconViewModel<StartMenuAppContextMenuItem>()
			{
				Text = vm.DesktopFile.Name,
				IconInfo = vm.Icon,
				ContextMenuItems = vm.DesktopFile.Actions
					.Select(a => new StartMenuAppContextMenuItem()
					{
						Id = a.DesktopFilePath,
						DisplayText = a.ActionName,
						Icon = new ImageViewModel() { IconNameOrPath = vm.DesktopFile.IconName },
						DesktopAction = a
					})
					.Concat([
						new StartMenuAppContextMenuItem() { DisplayText = "separator" },
						new StartMenuAppContextMenuItem()
						{
							Id = StartMenuAppContextMenuItem.ToggleStartMenuAppId,
							DisplayText = vm.IsPinnedToStartMenu ? "Unpin from Start" : "Pin to Start",
							Icon = new ImageViewModel() { IconNameOrPath = vm.IsPinnedToStartMenu ? "list-remove-symbolic" : "list-add-symbolic" },
							DesktopFile = vm.DesktopFile
						},
						new StartMenuAppContextMenuItem()
						{
							Id = StartMenuAppContextMenuItem.ToggleTaskbarAppId,
							DisplayText = vm.IsPinnedToTaskbar ? "Unpin from taskbar" : "Pin to taskbar",
							Icon = new ImageViewModel() { IconNameOrPath = vm.IsPinnedToTaskbar ? "list-remove-symbolic" : "list-add-symbolic" },
							DesktopFile = vm.DesktopFile
						}
					])
					.ToImmutableList()
			});

		var allAppsAppIcon = CreateAppIcon(applicationId, appIconViewModel);
		var pinnedAppsAppIcon = CreateAppIcon(applicationId, appIconViewModel);
		var searchAppsAppIcon = CreateAppIcon(applicationId, appIconViewModel);
		viewModelObs.SubscribeDebug(vm => _allApps.AddOrUpdate(allAppsAppIcon, vm.Index));
		viewModelObs.SubscribeDebug(vm => _pinnedApps.AddOrUpdate(pinnedAppsAppIcon, vm.PinnedIndex));
		searchIndexObs.SubscribeDebug(i => _searchApps.AddOrUpdate(searchAppsAppIcon, i));
		viewModelObs.CombineLatest(searchIndexObs).SubscribeDebug(t => _appIconViewModels[applicationId] = (ViewModel: t.First, SearchIndex: t.Second));
		_appIcons[applicationId] = (All: allAppsAppIcon, Pinned: pinnedAppsAppIcon, Search: searchAppsAppIcon);
	}

	public void HandleWindowShown()
	{
		_searchEntry.SetText("");
		_chipGroup.HideChip("search");
		_chipGroup.Select("pinned");
		_hiddenEntry.GrabFocus();
	}

	public bool HandleKeyPress(uint keyValue)
	{
		if (!_searchEntry.HasFocus() && _keyCodeRanges.Any(r => keyValue >= r.Item1 && keyValue <= r.Item2))
		{
			_searchEntry.GrabFocusWithoutSelecting();
		}
		else if (_hiddenEntry.HasFocus())
		{
			_searchEntry.GrabFocusWithoutSelecting();
			return true;
		}

		return false;
	}
}
