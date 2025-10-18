using System.Reactive.Linq;
using Glimpse.Libraries.System.Reactive;
using MentorLake.Redux;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;
using NCrontab;

namespace Glimpse.Libraries.Wallpaper;

internal static class InternalWallpaperActions
{
	public record UpdateConfigurationAction(WallpaperConfiguration Config);
	public record UpdateStateAction(WallpaperState State);
	public record WallpaperTickAction();
}

public static class WallpaperActions
{
	public record NextWallpaperAction();
}

internal class WallpaperEffects(ReduxStore store, WallpaperService wallpaperService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() =>
	[
		EffectsFactory.Create(actions => actions
			.OfType<InternalWallpaperActions.WallpaperTickAction>()
			.WithLatestFrom(
				store.Select(WallpaperSelectors.NextUpdate),
				store.Select(WallpaperSelectors.WallpaperState),
				store.Select(WallpaperSelectors.IsEnabled))
			.Where(t => t.Item4 && DateTime.Now >= t.Item2)
			.Select(_ => Observable.FromAsync(wallpaperService.UpdateWallpaperAsync))
			.Concat()
			.Select(_ => new InternalWallpaperActions.UpdateStateAction(new WallpaperState() { LastUpdate = DateTime.Now })),
			new() { Dispatch = true }),

		EffectsFactory.Create(actions => actions
			.OfType<WallpaperActions.NextWallpaperAction>()
			.Select(_ => Observable.FromAsync(wallpaperService.UpdateWallpaperAsync))
			.Concat()
			.Select(_ => new InternalWallpaperActions.UpdateStateAction(new WallpaperState() { LastUpdate = DateTime.Now })),
			new() { Dispatch = true }),

		EffectsFactory.Create(actions => actions
			.OfType<InternalWallpaperActions.UpdateStateAction>()
			.Select(action => Observable.FromAsync(() => wallpaperService.SaveStateAsync(action.State)))
			.Concat())
	];
}

internal class WallpaperReducers : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new WallpaperConfiguration())
			.On<InternalWallpaperActions.UpdateConfigurationAction>((s, a) => a.Config),
		FeatureReducer.Build(new WallpaperState())
			.On<InternalWallpaperActions.UpdateStateAction>((s, a) => a.State)
	];
}

internal static class WallpaperSelectors
{
	public static readonly ISelector<WallpaperConfiguration> WallpaperConfiguration = SelectorFactory.CreateFeature<WallpaperConfiguration>();
	public static readonly ISelector<WallpaperState> WallpaperState = SelectorFactory.CreateFeature<WallpaperState>();

	public static readonly ISelector<DateTime> NextUpdate = SelectorFactory.Create(
		WallpaperConfiguration,
		WallpaperState,
		(config, state) =>
		{
			var schedule = CrontabSchedule.Parse(config.Cron);
			return schedule.GetNextOccurrence(state.LastUpdate);
		});

	public static readonly ISelector<bool> IsEnabled = SelectorFactory.Create(WallpaperConfiguration, c => c.IsEnabled);
}
