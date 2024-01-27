using Glimpse.Common.Freedesktop.Xorg.State;
using Glimpse.Common.Freedesktop.Xorg.X11;
using Glimpse.Common.Microsoft.Extensions;
using MentorLake.Redux.Effects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Common.Freedesktop.Xorg;

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
