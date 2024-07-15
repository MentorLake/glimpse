using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;
using Monitor = Gdk.Monitor;

namespace Glimpse.Host;

public record UpdateMonitorsAction(IEnumerable<Monitor> Monitors);

public record GlimpseGtkState
{
	public IEnumerable<Monitor> Monitors { get; set; }
}

public static class GlimpseGtkSelectors
{
	private static ISelector<GlimpseGtkState> State =  SelectorFactory.CreateFeature<GlimpseGtkState>();
	public static readonly ISelector<IEnumerable<Monitor>> Monitors = SelectorFactory.Create(State, s => s.Monitors);
}

internal class GlimpseGtkReducerFactory : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new GlimpseGtkState())
			.On<UpdateMonitorsAction>((s, a) => s with { Monitors = a.Monitors })
	];
}
