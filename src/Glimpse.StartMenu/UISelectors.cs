using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.UI.State;

public static class UISelectors
{
	public static readonly ISelector<StartMenuState> StartMenuState = CreateFeature<StartMenuState>();
}
