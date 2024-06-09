using Glimpse.SidePane.Components.Calendar;
using Glimpse.SidePane.Components.SidePane;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.SidePane;

public static class SidePaneStartupExtensions
{
	public static Task UseSidePane(this IHost host)
	{
		return Task.CompletedTask;
	}

	public static void AddSidePane(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<CalendarWindow>();
		builder.Services.AddSingleton<SidePaneWindow>();
	}
}
