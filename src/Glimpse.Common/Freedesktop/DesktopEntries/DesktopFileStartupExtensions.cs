using System.Collections.Immutable;
using GLib;
using Glimpse.Common.Gtk;
using Glimpse.Common.Microsoft.Extensions;
using Glimpse.Common.System.Runtime.InteropServices;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.ObservableEvents;
using Task = System.Threading.Tasks.Task;

namespace Glimpse.Common.Freedesktop.DesktopEntries;

public static class DesktopFileStartupExtensions
{
	public static void AddDesktopFiles(this IHostApplicationBuilder builder)
	{
		builder.Services.AddInstance(DesktopFileReducers.AllReducers);
	}

	public static async Task UseDesktopFiles(this IHost host)
	{
		var store = host.Services.GetService<ReduxStore>();
		await LoadDesktopFiles(store);
		AppInfoMonitor.Get().Events().Changed.Subscribe(_ => LoadDesktopFiles(store));
	}

	private static async Task LoadDesktopFiles(ReduxStore store)
	{
		await store.Dispatch(new UpdateDesktopFilesAction() { DesktopFiles = AppInfoAdapter.GetAll().Where(a => a.ShouldShow).Select(CreateDesktopFile).ToImmutableList() });
	}

	private static DesktopFile CreateDesktopFile(IAppInfo a)
	{
		var filePath = LibGdk3Interop.g_desktop_app_info_get_filename(a.Handle);
		var actionNames = MarshalExtensions.PtrToStringArray(LibGdk3Interop.g_desktop_app_info_list_actions(a.Handle));

		var actions = actionNames
			.Select(actionId =>
			{
				var actionIdPtr = Marshaller.StringToPtrGStrdup(actionId);
				var actionNamePtr = LibGdk3Interop.g_desktop_app_info_get_action_name(a.Handle, actionIdPtr);
				var actionName = Marshaller.PtrToStringGFree(actionNamePtr);
				Marshaller.Free(actionIdPtr);
				return new DesktopFileAction() { Id = actionId, ActionName = actionName, DesktopFilePath = filePath };
			})
			.ToList();

		var desktopFile = new DesktopFile()
		{
			Id = filePath,
			FileName = Path.GetFileNameWithoutExtension(filePath),
			Name = a.Name,
			IconName = LibGdk3Interop.g_desktop_app_info_get_string(a.Handle, "Icon") ?? "",
			Executable = a.Executable,
			CommandLine = a.Commandline,
			StartupWmClass = LibGdk3Interop.g_desktop_app_info_get_startup_wm_class(a.Handle) ?? "",
			Actions = actions,
			Categories = ParseCategories(LibGdk3Interop.g_desktop_app_info_get_categories(a.Handle))
		};

		return desktopFile;
	}

	private static List<string> ParseCategories(string categories)
	{
		if (string.IsNullOrEmpty(categories)) return null;
		return categories.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
	}
}
