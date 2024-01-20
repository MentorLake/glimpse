using MentorLake.Redux.Effects;
using static MentorLake.Redux.Effects.EffectsFactory;

namespace Glimpse.Xorg.State;

internal class XorgEffects(IDisplayServer displayServer) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		CreateEffect<TakeScreenshotAction>(action => new[]
		{
			new UpdateScreenshotsAction()
			{
				Screenshots = action.Windows
					.Select(w => (w.Id, displayServer.TakeScreenshot(w))).Where(t => t.Item2 != null)
					.ToDictionary(t => t.Id, t => t.Item2)
			}
		}),
	};
}
