using System.Reactive.Linq;
using MentorLake.Redux.Effects;

namespace Glimpse.Libraries.Xorg.State;

internal class XorgEffects(IDisplayServer displayServer) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.Create(actions => actions
			.OfType<TakeScreenshotAction>()
			.Select(action => new UpdateScreenshotsAction()
			{
				Screenshots = action.Windows
					.Select(w => (w.Id, displayServer.TakeScreenshot(w))).Where(t => t.Item2 != null)
					.ToDictionary(t => t.Id, t => t.Item2)
			}),
			new EffectConfig() { Dispatch = true })
	};
}
