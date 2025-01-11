using System.Collections.Immutable;
using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.Services.StartMenu;

public static class StartMenuSelectors
{
	private static readonly ISelector<StartMenuState> s_startMenuState = CreateFeature<StartMenuState>();
	public static readonly ISelector<StartMenuConfiguration> Configuration = Create(s_startMenuState, s => s.Configuration);
	public static readonly ISelector<ImmutableList<string>> PinnedLaunchers = Create(Configuration, s => s.PinnedLaunchers);
	public static readonly ISelector<string> PowerButtonCommand = Create(Configuration, s => s.PowerButtonCommand);
	public static readonly ISelector<string> SettingsButtonCommand = Create(Configuration, s => s.SettingsButtonCommand);
	public static readonly ISelector<string> UserSettingsCommand = Create(Configuration, s => s.UserSettingsCommand);
	public static readonly ISelector<string> SearchTextSelector = Create(s_startMenuState, s => s.SearchText);
}
