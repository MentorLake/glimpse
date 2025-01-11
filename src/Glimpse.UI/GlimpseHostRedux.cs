using MentorLake.Gdk;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.UI;

public record UpdateMonitorsAction(IEnumerable<GdkMonitorHandle> Monitors);

public record GlimpseGtkState
{
	public IEnumerable<GdkMonitorHandle> Monitors { get; set; }
}

public static class GlimpseGtkSelectors
{
	private static ISelector<GlimpseGtkState> State =  SelectorFactory.CreateFeature<GlimpseGtkState>();
	public static readonly ISelector<IEnumerable<GdkMonitorHandle>> Monitors = SelectorFactory.Create(State, s => s.Monitors);
}

internal class GlimpseGtkReducerFactory : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new GlimpseGtkState())
			.On<UpdateMonitorsAction>((s, a) => s with { Monitors = a.Monitors })
	];
}
