using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.Microsoft.Extensions;
using MentorLake.Gio;

namespace Glimpse.Libraries.DesktopEntries;

public static class DesktopFileStartupExtensions
{
	public static void AddDesktopFiles(this IHostApplicationBuilder builder)
	{
		builder.Services.AddInstance(DesktopFileReducers.AllReducers);
	}

	public static async Task UseDesktopFiles(this IHost host)
	{
		var store = host.Services.GetService<ReduxStore>();

		LoadDesktopFiles(store);

		GAppInfoMonitorHandle.Get()
			.Signal_Changed()
			.ConflateLatest(TaskPoolScheduler.Default)
			.Subscribe(_ => LoadDesktopFiles(store));
	}

	private static void LoadDesktopFiles(ReduxStore store)
	{
		var desktopFiles = GAppInfoHandleExtensions.GetAll();

		store.Dispatch(new UpdateDesktopFilesAction()
		{
			DesktopFiles = desktopFiles
				.ToList<GDesktopAppInfoHandle>()
				.Where(a => a.ShouldShow())
				.Select(CreateDesktopFile)
				.ToImmutableList()
		});

		desktopFiles.FreeFull();
	}

	private static DesktopFile CreateDesktopFile(GDesktopAppInfoHandle a)
	{
		var filePath = a.GetFilename();
		var actionNames = a.ListActions();

		var actions = actionNames
			.Select(actionId => new DesktopFileAction() { Id = actionId, ActionName = a.GetActionName(actionId), DesktopFilePath = filePath })
			.ToList();

		var desktopFile = new DesktopFile()
		{
			Id = filePath,
			FileName = Path.GetFileNameWithoutExtension(filePath),
			Name = a.GetName(),
			IconName = a.GetString("Icon") ?? "",
			Executable = a.GetExecutable(),
			CommandLine = a.GetCommandline(),
			StartupWmClass = a.GetStartupWmClass() ?? "",
			Actions = actions,
			Categories = ParseCategories(a.GetCategories())
		};

		return desktopFile;
	}

	private static List<string> ParseCategories(string categories)
	{
		if (string.IsNullOrEmpty(categories)) return null;
		return categories.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
	}
}
