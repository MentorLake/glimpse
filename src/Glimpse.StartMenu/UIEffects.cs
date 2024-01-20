using Glimpse.Common.System.Collections.Immutable;
using Glimpse.Configuration;
using MentorLake.Redux.Effects;
using static MentorLake.Redux.Effects.EffectsFactory;

namespace Glimpse.UI.State;

public class UIEffects(ConfigurationService configurationService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		CreateEffect<ToggleStartMenuPinningAction, ConfigurationFile>(
			ConfigurationSelectors.Configuration,
			(a, s) =>
			{
				configurationService.UpdateConfiguration(s with { StartMenu = s.StartMenu with { PinnedLaunchers = s.StartMenu.PinnedLaunchers.Toggle(a.DesktopFileId) } });
			}),
		CreateEffect<UpdateStartMenuPinnedAppOrderingAction, ConfigurationFile>(
			ConfigurationSelectors.Configuration,
			(a, s) =>
			{
				if (s.StartMenu.PinnedLaunchers.SequenceEqual(a.DesktopFileKeys)) return;
				configurationService.UpdateConfiguration(s with { StartMenu = s.StartMenu with { PinnedLaunchers = a.DesktopFileKeys } });
			})
	};
}
