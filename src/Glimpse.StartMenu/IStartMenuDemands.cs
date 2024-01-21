using System.Collections.Immutable;
using MentorLake.Redux.Selectors;

namespace Glimpse.StartMenu;

public interface IStartMenuDemands
{
	void ToggleDesktopFilePinning(string desktopFileId);
	ISelector<ImmutableList<string>> TaskbarPinnedLaunchers { get; }
}
