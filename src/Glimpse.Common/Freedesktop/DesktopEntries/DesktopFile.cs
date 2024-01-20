using MentorLake.Redux;

namespace Glimpse.Freedesktop.DesktopEntries;

public class DesktopFile : IKeyed<string>
{
	public string Id { get; set; }
	public string FilePath => Id;
	public string Name { get; set; } = "";
	public string IconName { get; set; } = "";
	public string StartupWmClass { get; set; } = "";
	public string Executable { get; set; } = "";
	public List<DesktopFileAction> Actions { get; set; } = new();
	public List<string> Categories { get; set; } = new();
	public string CommandLine { get; set; }
	public string FileName { get; set; }
}
