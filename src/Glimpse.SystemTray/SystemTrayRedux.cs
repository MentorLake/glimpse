using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.SystemTray;

public record SystemTrayState
{
	public SystemTrayConfiguration Configuration { get; init; } = SystemTrayConfiguration.Empty;
}

public static class SystemTraySelectors
{
	public static readonly ISelector<SystemTrayState> SystemTrayState = SelectorFactory.CreateFeature<SystemTrayState>();
	public static readonly ISelector<SystemTrayConfiguration> Configuration = SelectorFactory.Create(SystemTrayState, s => s.Configuration);
	public static readonly ISelector<string> VolumeCommand = SelectorFactory.Create(Configuration, s => s.VolumeCommand);
}

public record UpdateSystemTrayConfiguration(SystemTrayConfiguration Config);

public record SystemTrayItemStateReducers : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new SystemTrayState())
			.On<UpdateSystemTrayConfiguration>((s, a) => s with { Configuration = a.Config })
	];
}
