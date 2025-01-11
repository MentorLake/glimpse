using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using MentorLake.GLib;
using MentorLake.Gtk3;

namespace Glimpse.Libraries.Gtk;

public class GLibExt
{
	public static SynchronizationContext SynchronizationContext { get; set; } = new GLibSynchronizationContext();
	public static IScheduler Scheduler { get; set; } = new SynchronizationContextScheduler(SynchronizationContext, false);

	public static void Defer(Action action, int priority = 100)
	{
		GSourceFunc function = static data =>
		{
			var gcHandle = GCHandle.FromIntPtr(data);
			var state = (Tuple<GSourceFunc, Action>) gcHandle.Target;
			state.Item2();
			gcHandle.Free();
			return false;
		};

		_ = GLibGlobalFunctions.IdleAddFull(priority, function, GCHandle.ToIntPtr(GCHandle.Alloc(Tuple.Create(function, action))), null);
	}
}
