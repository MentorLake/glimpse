using Glimpse.Common.Microsoft.Extensions;
using MentorLake.Redux.Effects;
using Glimpse.Xorg.State;
using Glimpse.Xorg.X11;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Xorg;

public static class XorgStartupExtensions
{
	public static Task UseXorg(this IHost host)
	{
		return Task.CompletedTask;
	}

	public static void AddXorg(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton<IEffectsFactory, XorgEffects>();
		builder.Services.AddSingleton<XLibAdaptorService>();
		builder.Services.AddSingleton<IDisplayServer, X11DisplayServer>();
		builder.Services.AddInstance(XorgReducers.Reducers);
		builder.Services.AddHostedService<XorgHostedService>();
	}
}
