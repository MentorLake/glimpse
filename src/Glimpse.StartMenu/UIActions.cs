using System.Collections.Immutable;

namespace Glimpse.UI.State;

public class StartMenuOpenedAction();
public record ToggleStartMenuPinningAction(string DesktopFileId);
public record UpdateStartMenuSearchTextAction(string SearchText);
public record UpdateStartMenuPinnedAppOrderingAction(ImmutableList<string> DesktopFileKeys);
public record UpdateAppFilteringChip(StartMenuChips Chip);
