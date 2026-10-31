using MentorLake.Redux.Thunks;

namespace Glimpse.Services.Taskbar;

public sealed class TaskbarThunkApi(ThunkApiContext context, ConfigurationService configurationService) : ThunkApi(context)
{
	public ConfigurationService ConfigurationService { get; } = configurationService;
}
