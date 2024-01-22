using Glimpse.Common.Images;

namespace Glimpse.StartMenu.Components;

public class Assets
{
	public static readonly IGlimpseImage Power;
	public static readonly IGlimpseImage Person;

	static Assets()
	{
		Power = LoadSvg("power-outline.svg");
		Person = LoadSvg("person-circle-outline.svg");
	}

	private static IGlimpseImage LoadSvg(string name)
	{
		return GlimpseImageFactory.FromResource(name);
	}
}
