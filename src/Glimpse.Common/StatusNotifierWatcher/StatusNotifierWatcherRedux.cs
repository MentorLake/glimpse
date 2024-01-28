using System.Collections.Immutable;
using Glimpse.Common.DBus;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Common.StatusNotifierWatcher;

public record StatusNotifierWatcherState
{
	public ImmutableDictionary<string, StatusNotifierWatcherItem> Items { get; init; } = ImmutableDictionary<string, StatusNotifierWatcherItem>.Empty;
}

public record StatusNotifierWatcherItem
{
	public StatusNotifierItemProperties Properties { get; init; }
	public DbusObjectDescription StatusNotifierItemDescription { get; init; }
	public DbusObjectDescription DbusMenuDescription { get; init; }
	public DbusMenuItem RootMenuItem { get; init; }
	public string GetServiceName() => StatusNotifierItemDescription.ServiceName;
}

public static class StatusNotifierWatcherSelectors
{
	public static readonly ISelector<StatusNotifierWatcherState> StatusNotifierWatcherState = SelectorFactory.CreateFeature<StatusNotifierWatcherState>();
}

internal record AddBulkTrayItemsAction(IEnumerable<StatusNotifierWatcherItem> Items);
internal record AddTrayItemAction(StatusNotifierWatcherItem Item);
internal record RemoveTrayItemAction(string ServiceName);
internal record UpdateMenuLayoutAction(string ServiceName, DbusMenuItem RootMenuItem);

internal record UpdateStatusNotifierItemPropertiesAction
{
	public string ServiceName { get; init; }
	public StatusNotifierItemProperties Properties { get; init; }
}

internal record StatusNotifierWatcherReducer : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new StatusNotifierWatcherState())
			.On<UpdateStatusNotifierItemPropertiesAction>((s, a) => s.Items.TryGetValue(a.ServiceName, out var currentItem)
				? s with { Items = s.Items.SetItem(a.ServiceName, currentItem with { Properties = a.Properties }) }
				: s)
			.On<UpdateMenuLayoutAction>((s, a) => s.Items.TryGetValue(a.ServiceName, out var currentItem)
				? s with { Items = s.Items.SetItem(a.ServiceName, currentItem with { RootMenuItem = a.RootMenuItem }) }
				: s)
			.On<AddBulkTrayItemsAction>((s, a) =>
			{
				var newItemList = new LinkedList<StatusNotifierWatcherItem>();

				foreach (var item in a.Items)
				{
					if (!s.Items.ContainsKey(item.GetServiceName()))
					{
						newItemList.AddLast(item);
					}
				}

				// Add existing items too

				return s with
				{
					Items = newItemList
						.DistinctBy(i => i.StatusNotifierItemDescription.ServiceName)
						.ToImmutableDictionary(i => i.StatusNotifierItemDescription.ServiceName, i => i)
				};
			})
			.On<AddTrayItemAction>((s, a) => !s.Items.ContainsKey(a.Item.GetServiceName())
				? s with { Items = s.Items.Add(a.Item.GetServiceName(), a.Item) }
				: s)
			.On<RemoveTrayItemAction>((s, a) => s.Items.ContainsKey(a.ServiceName)
				? s with { Items = s.Items.Remove(a.ServiceName) }
				: s)
	];
}
