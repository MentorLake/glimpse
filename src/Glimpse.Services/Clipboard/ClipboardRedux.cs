namespace Glimpse.Services.Clipboard;

public record ClipboardHistory
{
	public ImmutableList<ClipboardEntry> Entries { get; set; } = ImmutableList<ClipboardEntry>.Empty;
}

public record UpdateClipboardConfigurationAction(ClipboardConfiguration Config);
public record LoadClipboardHistoryAction(ClipboardHistory History);
public record CaptureClipboardItemAction(string Text);
public record PinClipboardItemAction(Guid Id);
public record RemoveClipboardItemAction(Guid Id);
public record ClearClipboardHistoryAction();
public record ClipboardPickerOpenedAction();

public static class ClipboardSelectors
{
	public static readonly ISelector<ClipboardConfiguration> Configuration = SelectorFactory.CreateFeature<ClipboardConfiguration>();
	public static readonly ISelector<ClipboardHistory> History = SelectorFactory.CreateFeature<ClipboardHistory>();
	public static readonly ISelector<ImmutableList<ClipboardEntry>> Entries = SelectorFactory.Create(History, history => history.Entries);
}

internal class ClipboardReducers
{
	public static readonly FeatureReducerCollection AllReducers = new()
	{
		FeatureReducer.Build(new ClipboardConfiguration())
			.On<UpdateClipboardConfigurationAction>((s, a) => a.Config),

		FeatureReducer.Build(new ClipboardHistory())
			.On<LoadClipboardHistoryAction>((s, a) => a.History)
			.On<ClearClipboardHistoryAction>((s, _) => s with { Entries = s.Entries.Where(e => e.IsPinned).ToImmutableList() })
			.On<RemoveClipboardItemAction>((s, a) => s with { Entries = s.Entries.Where(e => e.Id != a.Id).ToImmutableList() })
			.On<PinClipboardItemAction>((s, a) => s with
			{
				Entries = s.Entries.Select(e => e.Id == a.Id ? e with { IsPinned = !e.IsPinned } : e).ToImmutableList()
			})
			.On<CaptureClipboardItemAction>((s, a) => ApplyCapture(s, a.Text, ClipboardConfiguration.Empty.MaxEntries))
	};

	public static ClipboardHistory ApplyCapture(ClipboardHistory history, string text, int maxEntries)
	{
		var entry = ClipboardEntry.Create(text);

		if (entry == null)
		{
			return history;
		}

		var existing = history.Entries.FirstOrDefault(e => e.Text == entry.Text);

		if (existing != null)
		{
			var withoutExisting = history.Entries.Remove(existing);
			return history with { Entries = withoutExisting.Insert(0, existing with { CapturedAt = entry.CapturedAt }) };
		}

		var updated = history.Entries.Insert(0, entry);
		return history with { Entries = Evict(updated, maxEntries) };
	}

	private static ImmutableList<ClipboardEntry> Evict(ImmutableList<ClipboardEntry> entries, int maxEntries)
	{
		if (entries.Count <= maxEntries)
		{
			return entries;
		}

		var evictableIndexes = Enumerable
			.Range(0, entries.Count)
			.Where(i => !entries[i].IsPinned)
			.OrderByDescending(i => i)
			.Take(entries.Count - maxEntries)
			.ToHashSet();

		if (evictableIndexes.Count == 0)
		{
			return entries;
		}

		return entries.Where((_, i) => !evictableIndexes.Contains(i)).ToImmutableList();
	}
}

internal class ClipboardEffects(ReduxStore store, ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.Create(actions => actions
			.Where(a => a is UpdateClipboardConfigurationAction)
			.WithLatestFrom(store.Select(ClipboardSelectors.Configuration))
			.Do(t =>
			{
				var (_, config) = t;
				var serializedConfig = JsonSerializer.SerializeToNode(config, typeof(ClipboardConfiguration), ClipboardSerializationContext.Instance)?.AsObject();
				configurationService.Upsert(ClipboardConfiguration.ConfigKey, serializedConfig);
			}))
	};
}
