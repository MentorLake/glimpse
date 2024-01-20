using Glimpse.Common.Images;
using MentorLake.Redux;
using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.Xorg.State;

public class XorgSelectors
{
	public static readonly ISelector<DataTable<ulong, IGlimpseImage>> Screenshots = CreateFeature<DataTable<ulong, IGlimpseImage>>();
	public static readonly ISelector<DataTable<ulong, WindowProperties>> Windows = CreateFeature<DataTable<ulong, WindowProperties>>();
}
