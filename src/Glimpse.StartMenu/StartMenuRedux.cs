using System.Collections.Immutable;
using System.Text.Json;
using Glimpse.Common.System.Collections.Immutable;
using Glimpse.Configuration;
using Glimpse.UI.State;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;

namespace Glimpse.StartMenu;

public record StartMenuState
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

public enum StartMenuChips
{
	Pinned,
	AllApps,
	SearchResults
}

public class StartMenuOpenedAction();
public record ToggleStartMenuPinningAction(string DesktopFileId);
public record UpdateStartMenuSearchTextAction(string SearchText);
public record UpdateStartMenuPinnedAppOrderingAction(ImmutableList<string> DesktopFileKeys);
public record UpdateAppFilteringChip(StartMenuChips Chip);
public record UpdateStartMenuConfiguration(StartMenuConfiguration Config);

public class StartMenuEffects(ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.CreateEffect<ToggleStartMenuPinningAction, StartMenuConfiguration>(
			StartMenuSelectors.s_configuration,
			(a, s) =>
			{
				var updatedConfig = s with { PinnedLaunchers = s.PinnedLaunchers.Toggle(a.DesktopFileId) };
				var serializedConfig = JsonSerializer.SerializeToElement(updatedConfig, typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance);
				configurationService.Upsert(StartMenuConfiguration.ConfigKey, serializedConfig);
			}),
		EffectsFactory.CreateEffect<UpdateStartMenuPinnedAppOrderingAction, StartMenuConfiguration>(
			StartMenuSelectors.s_configuration,
			(a, s) =>
			{
				if (!s.PinnedLaunchers.SequenceEqual(a.DesktopFileKeys))
				{
					var updatedConfig = s with { PinnedLaunchers = a.DesktopFileKeys };
					var serializedConfig = JsonSerializer.SerializeToElement(updatedConfig, typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance);
					configurationService.Upsert(StartMenuConfiguration.ConfigKey, serializedConfig);
				}
			})
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
