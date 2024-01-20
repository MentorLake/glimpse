using Glimpse.Common.Microsoft.Extensions;
using MentorLake.Redux.Effects;
using Glimpse.UI.Components.StartMenu;
using Glimpse.UI.Components.StartMenu.Window;
using Glimpse.UI.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.UI;

public static class StartMenuStartupExtensions
{
	public static Task UseStartMenu(this IHost host)
	{
		return Task.CompletedTask;
	}

	public static void AddStartMenu(this IHostApplicationBuilder builder)
	{
		builder.Services.AddTransient<StartMenuLaunchIcon>();
		builder.Services.AddSingleton<StartMenuWindow>();
		builder.Services.AddInstance(UIReducers.AllReducers);
		builder.Services.AddSingleton<IEffectsFactory, UIEffects>();
	}
}
