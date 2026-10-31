namespace Glimpse.Services.Taskbar;

public record UpdateTaskbarConfigurationAction(TaskbarConfiguration Configuration);
public record ToggleTaskbarPinningAction(string DesktopFileId);
public record UpdateTaskbarSlotOrderingAction(ImmutableList<SlotRef> Slots);
