using System.Reactive.Concurrency;
using GLib;

namespace Glimpse.Common.Gtk;

public class GLibExt
{
	public static SynchronizationContext SynchronizationContext { get; set; } = new GLibSynchronizationContext();
	public static IScheduler Scheduler { get; set; } = new SynchronizationContextScheduler(SynchronizationContext, false);
}
