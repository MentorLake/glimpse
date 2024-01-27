using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Text.Json;
using Glimpse.Common.Configuration;
using Glimpse.Common.Gtk;
using Glimpse.Common.System.Collections.Immutable;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;

namespace Glimpse.StartMenu;

internal record StartMenuState
{
	public string SearchText { get; init; } = "";
	public StartMenuConfiguration Configuration { get; set; } = StartMenuConfiguration.Empty;

	public ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip> Chips { get; init; } =
		ImmutableDictionary<StartMenuChips, StartMenuAppFilteringChip>.Empty
			.Add(StartMenuChips.Pinned, new() { IsSelected = true, IsVisible = true })
			.Add(StartMenuChips.AllApps, new() { IsSelected = false, IsVisible = true })
			.Add(StartMenuChips.SearchResults, new() { IsSelected = false, IsVisible = false });

	public virtual bool Equals(StartMenuState other) => ReferenceEquals(this, other);
}

internal enum StartMenuChips
{
	Pinned,
	AllApps,
	SearchResults
}

public record StartMenuOpenedAction();
internal record ToggleStartMenuPinningAction(string DesktopFileId);
internal record UpdateStartMenuSearchTextAction(string SearchText);
internal record UpdateStartMenuPinnedAppOrderingAction(ImmutableList<string> DesktopFileKeys);
internal record UpdateAppFilteringChip(StartMenuChips Chip);
internal record UpdateStartMenuConfiguration(StartMenuConfiguration Config);

internal class StartMenuEffects(ReduxStore store, ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.Create(actions => actions
			.OfType<ToggleStartMenuPinningAction>()
			.WithLatestFrom(store.Select(StartMenuSelectors.s_configuration))
			.Do(t =>
			{
				var (a, s) = t;
				var updatedConfig = s with { PinnedLaunchers = s.PinnedLaunchers.Toggle(a.DesktopFileId) };
				var serializedConfig = JsonSerializer.SerializeToNode(updatedConfig, typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance)?.AsObject();
				configurationService.Upsert(StartMenuConfiguration.ConfigKey, serializedConfig);
			})),
		EffectsFactory.Create(actions => actions
			.OfType<UpdateStartMenuPinnedAppOrderingAction>()
			.WithLatestFrom(store.Select(StartMenuSelectors.s_configuration))
			.Do(t =>
			{
				var (a, s) = t;

				if (!s.PinnedLaunchers.SequenceEqual(a.DesktopFileKeys))
				{
					var updatedConfig = s with { PinnedLaunchers = a.DesktopFileKeys };
					var serializedConfig = JsonSerializer.SerializeToNode(updatedConfig, typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance)?.AsObject();
					configurationService.Upsert(StartMenuConfiguration.ConfigKey, serializedConfig);
				}
			}))
	};
}

internal class StartMenuReducers
{
	public static readonly FeatureReducerCollection AllReducers = new()
	{
		FeatureReducer.Build(new StartMenuState())
			.On<UpdateStartMenuConfiguration>((s, a) => s with { Configuration = a.Config })
			.On<UpdateAppFilteringChip>((s, a) =>
			{
				var chips = s.Chips;
				chips = chips.SetItem(StartMenuChips.Pinned, chips[StartMenuChips.Pinned] with { IsSelected = a.Chip == StartMenuChips.Pinned });
				chips = chips.SetItem(StartMenuChips.AllApps, chips[StartMenuChips.AllApps] with { IsSelected = a.Chip == StartMenuChips.AllApps });
				chips = chips.SetItem(StartMenuChips.SearchResults, chips[StartMenuChips.SearchResults] with { IsSelected = a.Chip == StartMenuChips.SearchResults });
				return s with { Chips = chips };
			})
			.On<UpdateStartMenuSearchTextAction>((s, a) =>
			{
				var chips = s.Chips;

				if (string.IsNullOrEmpty(a.SearchText))
				{
					chips = chips.SetItem(StartMenuChips.Pinned, new StartMenuAppFilteringChip { IsSelected = true, IsVisible = true });
					chips = chips.SetItem(StartMenuChips.AllApps, new StartMenuAppFilteringChip { IsSelected = false, IsVisible = true });
					chips = chips.SetItem(StartMenuChips.SearchResults, new StartMenuAppFilteringChip { IsSelected = false, IsVisible = false });
				}
				else
				{
					chips = chips.SetItem(StartMenuChips.Pinned, new StartMenuAppFilteringChip { IsSelected = false, IsVisible = true });
					chips = chips.SetItem(StartMenuChips.AllApps, new StartMenuAppFilteringChip { IsSelected = false, IsVisible = true });
					chips = chips.SetItem(StartMenuChips.SearchResults, new StartMenuAppFilteringChip { IsSelected = true, IsVisible = true });
				}

				return s with { SearchText = a.SearchText, Chips = chips };
			})
	};
}
