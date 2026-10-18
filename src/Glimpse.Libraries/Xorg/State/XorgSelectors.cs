namespace Glimpse.Libraries.Xorg.State;

public class XorgSelectors
{
	public static readonly ISelector<DataTable<ulong, GdkPixbufHandle>> Screenshots = CreateFeature<DataTable<ulong, GdkPixbufHandle>>();
	public static readonly ISelector<DataTable<ulong, WindowProperties>> Windows = CreateFeature<DataTable<ulong, WindowProperties>>();
	public static readonly ISelector<ImmutableList<MonitorInfo>> Monitors = CreateFeature<ImmutableList<MonitorInfo>>();
}
