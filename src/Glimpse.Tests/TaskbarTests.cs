using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text.Json.Nodes;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.DesktopEntries;
using Glimpse.Libraries.Xorg;
using Glimpse.Libraries.Xorg.State;
using Glimpse.Services.Taskbar;
using MentorLake.Redux;
using MentorLake.Redux.Reducers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glimpse.Tests;

[TestClass]
[DoNotParallelize]
public sealed class TaskbarTests
{
	[TestMethod]
	public async Task UpdateConfiguration_UnchangedConfiguration_RetainsTransientSlot()
	{
		await using var context = new TestContext();
		var left = new SlotRef { PinnedDesktopFileId = "left" };
		var runningApp = new SlotRef { ClassHintName = "running-app", DiscoveredDesktopFileId = "running" };
		var right = new SlotRef { PinnedDesktopFileId = "right" };
		var initialSlots = ImmutableList.Create(left, runningApp, right);
		context.AddWindow("running", 1, "running-app");
		await context.Order(initialSlots);

		await context.Configure(Configuration("left", "", "right") with { TaskManagerCommand = "updated-command" });

		await AssertSlotOrder(context.Store, initialSlots, "Unchanged configuration moved or removed a transient slot.");
		var configuration = await GetConfiguration(context.Store);
		AssertSequence(new[] { "left", "right" }, configuration.PinnedLaunchers, "Unchanged configuration retained empty pin identifiers.");
		Assert.AreEqual("updated-command", configuration.TaskManagerCommand, "Non-pin configuration was not updated.");
	}

	[TestMethod]
	public async Task UpdateSlotOrdering_DraggingPinAcrossRunningApps_RetainsFullOrder()
	{
		await using var context = new TestContext();
		var left = new SlotRef { PinnedDesktopFileId = "left" };
		var runningApp = new SlotRef { ClassHintName = "running-app", DiscoveredDesktopFileId = "running" };
		var right = new SlotRef { PinnedDesktopFileId = "right" };
		await context.Configure(Configuration("left", "right"));

		var draggedSlots = ImmutableList.Create(runningApp, right, left);
		context.AddWindow("running", 1, "running-app");
		await context.Order(draggedSlots);
		AssertSequence(new[] { "right", "left" }, (await GetConfiguration(context.Store)).PinnedLaunchers, "Ordering did not immediately update configured pins.");
		var persistedConfiguration = await context.ReadConfiguration();
		AssertSequence(new[] { "right", "left" }, persistedConfiguration.PinnedLaunchers, "Ordering did not persist only the pinned launchers in order.");
		await context.Configure(persistedConfiguration);
		await AssertSlotOrder(context.Store, draggedSlots, "Dragging a pin across a running app did not preserve the complete displayed order.");
	}

	[TestMethod]
	public async Task UpdateSlotOrdering_MetadataRefresh_RetainsRightHandSlotIdentity()
	{
		await using var context = new TestContext();
		var left = new SlotRef { PinnedDesktopFileId = "left" };
		var runningApp = new SlotRef { ClassHintName = "running-app", DiscoveredDesktopFileId = "running" };
		var right = new SlotRef { PinnedDesktopFileId = "right", ClassHintName = "old-class" };
		context.AddWindow("running", 1, "running-app");
		await context.Order(ImmutableList.Create(left, runningApp, right));

		var refreshedRight = right with { ClassHintName = "new-class", DiscoveredDesktopFileId = "right" };
		var refreshedSlots = ImmutableList.Create(left, runningApp, refreshedRight);
		await context.Order(refreshedSlots);
		await context.Configure(Configuration("left", "right"));

		await AssertSlotOrder(context.Store, refreshedSlots, "A metadata refresh changed the position or identity of a right-hand pinned slot.");
	}

	[TestMethod]
	public async Task UpdateConfiguration_InitialPinConfiguration_CreatesSlots()
	{
		await using var context = new TestContext();
		await context.Configure(Configuration("first", "", "second"));

		var slots = (await GetSlots(context.Store)).Refs;
		AssertSequence(new[] { "first", "second" }, slots.Select(slot => slot.PinnedDesktopFileId), "Initial configuration did not create pinned slots in order.");
		AssertSequence(new[] { "first", "second" }, (await GetConfiguration(context.Store)).PinnedLaunchers, "Initial configuration did not filter empty pin identifiers.");
	}

	[TestMethod]
	public async Task UpdateConfiguration_ExternalConfigurationChanges_ReconcilesSlots()
	{
		await using var context = new TestContext();
		await context.Configure(Configuration("first", "second"));
		var initial = (await GetSlots(context.Store)).Refs;
		await context.Configure(Configuration("second", "third"));
		var slots = (await GetSlots(context.Store)).Refs;
		AssertSequence(new[] { "second", "third" }, slots.Select(slot => slot.PinnedDesktopFileId), "External pin changes were not reconciled.");
		Assert.AreEqual(initial[1], slots[0], "External pin changes replaced a retained slot.");
		await context.Configure(Configuration());
		await AssertSlotOrder(context.Store, ImmutableList<SlotRef>.Empty, "Removing all configured pins did not remove launcher slots.");
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	public async Task TogglePinning_UnpinningLaunchedLauncher_RetainsPosition(int index)
	{
		await using var context = new TestContext();
		var launcher = new SlotRef { PinnedDesktopFileId = "launcher" };
		var running = new SlotRef { ClassHintName = "running", DiscoveredDesktopFileId = "running" };
		var right = new SlotRef { PinnedDesktopFileId = "right" };
		var initialSlots = ImmutableList.Create(running, right).Insert(index, launcher);
		await context.Order(initialSlots);
		context.AddWindow("running", 1);
		context.AddWindow("launcher", 2);

		await context.Toggle("launcher");
		await context.Configure(await context.ReadConfiguration());
		var unpinned = launcher with { PinnedDesktopFileId = "", ClassHintName = "launcher", DiscoveredDesktopFileId = "launcher" };
		var unpinnedSlots = initialSlots.Replace(launcher, unpinned);
		await AssertSlotOrder(context.Store, unpinnedSlots, "Unpinning a launched launcher moved or replaced its slot.");
		AssertSequence(new[] { "right" }, (await context.ReadConfiguration()).PinnedLaunchers, "Unpinning a launched launcher did not persist the remaining pins.");

		var windows = (await context.Store.Select(XorgSelectors.Windows).FirstAsync().ToTask()).ById.Values.ToImmutableList();
		var displayed = await context.Store.Select(TaskbarSelectors.CurrentSlots).FirstAsync().ToTask();
		CollectionAssert.AreEqual(unpinnedSlots.ToArray(), displayed.Refs.ToArray(), "Resolving the displayed slots moved the unpinned launcher.");
		context.Store.Dispatch(new RemoveWindowAction { WindowProperties = windows.First(w => w.ClassHintName == "launcher") });
		var afterClosing = await context.Store.Select(TaskbarSelectors.CurrentSlots).FirstAsync().ToTask();
		CollectionAssert.AreEqual(new[] { running, right }, afterClosing.Refs.ToArray(), "Closing an unpinned launcher left a visible slot.");
	}

	[TestMethod]
	public async Task TogglePinning_NewRunningSlot_PreservesSelectedSlotIdentity()
	{
		await using var context = new TestContext();
		context.AddWindow("running", 1);
		var displayed = await context.Store.Select(TaskbarSelectors.CurrentSlots).FirstAsync().ToTask();
		var runningSlot = displayed.Refs.Single();

		await context.Toggle("running");

		await AssertSlotOrder(context.Store, ImmutableList.Create(runningSlot with { PinnedDesktopFileId = "running" }), "Pinning rebuilt the selected running slot instead of preserving its identity.");
	}

	[TestMethod]
	public async Task TogglePinning_RunningSlot_PreservesSlot()
	{
		await using var context = new TestContext();
		var left = new SlotRef { PinnedDesktopFileId = "left" };
		var runningApp = new SlotRef { ClassHintName = "running-app", DiscoveredDesktopFileId = "running" };
		var right = new SlotRef { PinnedDesktopFileId = "right" };
		await context.Order(ImmutableList.Create(left, runningApp, right));
		context.AddWindow("running", 1, "running-app");

		await context.Toggle("running");
		await AssertSlotOrder(context.Store, ImmutableList.Create(left, runningApp with { PinnedDesktopFileId = "running" }, right), "Pinning replaced or moved a running slot.");
		AssertSequence(new[] { "left", "running", "right" }, (await context.ReadConfiguration()).PinnedLaunchers, "Pinning did not persist launcher order.");

		await context.Toggle("running");
		await context.Configure(await context.ReadConfiguration());
		await AssertSlotOrder(context.Store, ImmutableList.Create(left, runningApp, right), "Unpinning removed or moved a running slot.");
		AssertSequence(new[] { "left", "right" }, (await GetConfiguration(context.Store)).PinnedLaunchers, "Unpinning left stale configured pins.");
	}

	[TestMethod]
	public async Task TogglePinning_IdleLauncher_AddsAndRemovesLauncher()
	{
		await using var context = new TestContext();
		await context.Toggle("launcher");
		AssertSequence(new[] { "launcher" }, (await GetSlots(context.Store)).Refs.Select(slot => slot.PinnedDesktopFileId), "Pinning did not create a launcher slot.");
		AssertSequence(new[] { "launcher" }, (await context.ReadConfiguration()).PinnedLaunchers, "New launcher was not persisted.");
		await context.Toggle("launcher");
		await AssertSlotOrder(context.Store, ImmutableList<SlotRef>.Empty, "Unpinning an idle launcher did not remove its slot.");
		AssertSequence(Array.Empty<string>(), (await context.ReadConfiguration()).PinnedLaunchers, "Removed launcher was still persisted.");
	}

	[TestMethod]
	public async Task CurrentSlots_Recomputation_RetainsGeneratedIdentitiesWithoutStoringDerivedSlots()
	{
		await using var context = new TestContext();
		await context.Configure(Configuration("launcher"));
		context.AddWindow("running", 1);
		var initial = await GetSlots(context.Store);
		context.Store.Dispatch(new AddWindowAction(new WindowProperties { WindowRef = new TestWindowRef(2), ClassHintName = "other" }));
		var refreshed = await GetSlots(context.Store);
		AssertSequence(initial.Refs.Select(s => s.Id), refreshed.Refs.Take(2).Select(s => s.Id), "Recomputation replaced existing slots.");
		var stored = await context.Store.Select(TaskbarSelectors.s_storedSlots).FirstAsync().ToTask();
		Assert.IsEmpty(stored.Refs, "Configuration or window updates stored derived slots.");
	}

	[TestMethod]
	public async Task UpdateConfiguration_AfterOrdering_ChangesOnlyAuthoritativePinState()
	{
		await using var context = new TestContext();
		context.AddWindow("running", 1);
		await context.Configure(Configuration("first", "second"));
		await context.Order((await GetSlots(context.Store)).Refs);
		var stored = await context.Store.Select(TaskbarSelectors.s_storedSlots).FirstAsync().ToTask();
		Assert.IsTrue(stored.Refs.All(s => string.IsNullOrEmpty(s.PinnedDesktopFileId)), "Ordering retained duplicate pin state.");
		await context.Configure(Configuration("second", "third"));
		Assert.AreSame(stored, await context.Store.Select(TaskbarSelectors.s_storedSlots).FirstAsync().ToTask(), "Configuration reconciled slots in the reducer.");
		AssertSequence(new[] { "second", "third" }, (await GetSlots(context.Store)).Refs.Where(s => s.PinnedDesktopFileId != "").Select(s => s.PinnedDesktopFileId), "Selector did not reconcile configured pins.");
	}

	private sealed class TestContext : IAsyncDisposable
	{
		private readonly string _directory = Path.Combine(Directory.GetCurrentDirectory(), $"taskbar-regression-{Guid.NewGuid():N}");
		private readonly string _path;
		private readonly Subject<Unit> _disposeSubject = new();

		public ReduxStore Store { get; } = new();

		public TestContext()
		{
			_path = Path.Combine(_directory, "config.json");
			var configurationService = new ConfigurationService(NullLogger<ConfigurationService>.Instance);
			configurationService.Initialize(_path);
			Store.RegisterReducers([TaskbarReducers.AllReducers]);
			var builder = Host.CreateApplicationBuilder();
			builder.AddXorg();
			using var services = builder.Services.BuildServiceProvider();
			Store.RegisterReducers([.. services.GetServices<FeatureReducerCollection>()]);
			Store.RegisterReducers([DesktopFileReducers.AllReducers]);

			Observable
				.Select(Store.Select(TaskbarSelectors.s_configuration), configuration => configuration.ToJsonElement())
				.DistinctUntilChanged(EqualityComparer<JsonObject>.Create(JsonNode.DeepEquals))
				.Skip(1)
				.TakeUntil(_disposeSubject)
				.Subscribe(configuration => configurationService.Upsert(TaskbarConfiguration.ConfigKey, configuration));
		}

		public void AddWindow(string desktopFileId, ulong id, string classHintName = null)
		{
			classHintName ??= desktopFileId;
			Store.Dispatch(new UpdateDesktopFilesAction { DesktopFiles = ImmutableList.Create(new DesktopFile { Id = desktopFileId, Name = classHintName, FileName = desktopFileId }) });
			Store.Dispatch(new AddWindowAction(new WindowProperties { WindowRef = new TestWindowRef(id), ClassHintName = classHintName }));
		}

		public Task Configure(TaskbarConfiguration configuration)
		{
			Store.Dispatch(new UpdateTaskbarConfigurationAction(configuration));
			return Task.CompletedTask;
		}
		public Task Order(ImmutableList<SlotRef> slots)
		{
			Store.Dispatch(new UpdateTaskbarSlotOrderingAction(slots));
			return Task.CompletedTask;
		}
		public Task Toggle(string desktopFileId)
		{
			Store.Dispatch(new ToggleTaskbarPinningAction(desktopFileId));
			return Task.CompletedTask;
		}

		public async Task<TaskbarConfiguration> ReadConfiguration() => TaskbarConfiguration.From(JsonNode.Parse(await File.ReadAllTextAsync(_path))![TaskbarConfiguration.ConfigKey]!.AsObject());

		public async ValueTask DisposeAsync()
		{
			await Store.DisposeAsync();
			_disposeSubject.OnNext(Unit.Default);
			_disposeSubject.Dispose();
			if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
		}
	}

	private sealed record TestWindowRef(ulong Id) : IWindowRef;

	private static TaskbarConfiguration Configuration(params string[] pinnedLaunchers) => new()
	{
		PinnedLaunchers = pinnedLaunchers.ToImmutableList()
	};

	private static async Task<SlotReferences> GetSlots(ReduxStore store) => await store.Select(TaskbarSelectors.CurrentSlots).FirstAsync().ToTask();

	private static async Task<TaskbarConfiguration> GetConfiguration(ReduxStore store) => await store.Select(TaskbarSelectors.s_configuration).FirstAsync().ToTask();

	private static async Task AssertSlotOrder(ReduxStore store, ImmutableList<SlotRef> expected, string message)
	{
		var actual = (await GetSlots(store)).Refs;
		AssertSequence(expected.Select(slot => slot.Id), actual.Select(slot => slot.Id), message);
		CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), $"{message} Slot metadata was not preserved.");
	}

	private static void AssertSequence(IEnumerable<string> expected, IEnumerable<string> actual, string message)
	{
		CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), message);
	}
}
