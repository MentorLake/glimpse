using System.Reflection;
using Glimpse.Libraries.Images;
using MentorLake.GdkPixbuf;

namespace Glimpse.UI.Components.StartMenu;

internal static class Assets
{
	public static readonly GdkPixbufHandle Power;
	public static readonly GdkPixbufHandle Person;

	static Assets()
	{
		Power = LoadSvg("power-outline.svg");
		Person = LoadSvg("person-circle-outline.svg");
	}

	private static GdkPixbufHandle LoadSvg(string name)
	{
		return GdkPixbufFactory.FromResource(Assembly.GetExecutingAssembly(), name);
	}
}
