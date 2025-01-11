using System.Reflection;
using System.Text;

namespace Glimpse.Libraries.System.Reflection;

public static class AssemblyExtensions
{
	public static string ConcatAllManifestFiles(this IEnumerable<Assembly> assemblies, string extension)
	{
		var contents = new StringBuilder();

		foreach (var assembly in assemblies)
		{
			foreach (var fileName in assembly.GetManifestResourceNames().Where(n => n.EndsWith("." + extension)))
			{
				using var cssFileStream = new StreamReader(assembly.GetManifestResourceStream(fileName));
				contents.AppendLine(cssFileStream.ReadToEnd());
			}
		}

		return contents.ToString();
	}
}
