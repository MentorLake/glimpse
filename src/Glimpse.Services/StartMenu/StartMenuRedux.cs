using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Text.Json;
using Glimpse.Libraries.Configuration;
using Glimpse.Libraries.System.Collections.Immutable;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;

namespace Glimpse.Services.StartMenu;

public record StartMenuState
{
	public string SearchText { get; init; } = "";
	public StartMenuConfiguration Configuration { get; set; } = StartMenuConfiguration.Empty;
	public virtual bool Equals(StartMenuState other) => ReferenceEquals(this, other);
}

public record StartMenuOpenedAction();
public record ToggleStartMenuPinningAction(string DesktopFileId);
public record UpdateStartMenuSearchTextAction(string SearchText);
public record UpdateStartMenuPinnedAppOrderingAction(ImmutableList<string> DesktopFileKeys);
public record UpdateStartMenuConfiguration(StartMenuConfiguration Config);

internal class StartMenuEffects(ReduxStore store, ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.Create(actions => actions
			.OfType<ToggleStartMenuPinningAction>()
			.WithLatestFrom(store.Select(StartMenuSelectors.Configuration))
			.Do(t =>
			{
				var (a, s) = t;
				var updatedConfig = s with { PinnedLaunchers = s.PinnedLaunchers.Toggle(a.DesktopFileId) };
				var serializedConfig = JsonSerializer.SerializeToNode(updatedConfig, typeof(StartMenuConfiguration), StartMenuSerializationContext.Instance)?.AsObject();
				configurationService.Upsert(StartMenuConfiguration.ConfigKey, serializedConfig);
			})),
		EffectsFactory.Create(actions => actions
			.OfType<UpdateStartMenuPinnedAppOrderingAction>()
			.WithLatestFrom(store.Select(StartMenuSelectors.Configuration))
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
			.On<UpdateStartMenuSearchTextAction>((s, a) => s with { SearchText = a.SearchText })
	};
}
