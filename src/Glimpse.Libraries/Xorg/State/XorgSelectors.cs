using MentorLake.GdkPixbuf;
using MentorLake.Redux;
using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.Libraries.Xorg.State;

public class XorgSelectors
{
	public static readonly ISelector<DataTable<ulong, GdkPixbufHandle>> Screenshots = CreateFeature<DataTable<ulong, GdkPixbufHandle>>();
	public static readonly ISelector<DataTable<ulong, WindowProperties>> Windows = CreateFeature<DataTable<ulong, WindowProperties>>();
}
