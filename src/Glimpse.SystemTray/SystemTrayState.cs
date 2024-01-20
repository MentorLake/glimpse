using System.Collections.Immutable;
using Glimpse.Freedesktop.DBus;
using Glimpse.Freedesktop.DBus.Introspection;
using Glimpse.Freedesktop.SystemTray;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using static MentorLake.Redux.Effects.EffectsFactory;

namespace Glimpse.Freedesktop;

public class SystemTrayState
{
	public ImmutableDictionary<string, SystemTrayItemState> Items { get; init; }= ImmutableDictionary<string, SystemTrayItemState>.Empty;
}

public class SystemTrayItemState
{
	public StatusNotifierItemProperties Properties { get; init; }
	public DbusObjectDescription StatusNotifierItemDescription { get; init; }
	public DbusObjectDescription DbusMenuDescription { get; init; }
	public DbusMenuItem RootMenuItem { get; init; }

	public SystemTrayItemState()
	{

	}

	public SystemTrayItemState(SystemTrayItemState other)
	{
		Properties = other.Properties;
		StatusNotifierItemDescription = other.StatusNotifierItemDescription;
		DbusMenuDescription = other.DbusMenuDescription;
		RootMenuItem = other.RootMenuItem;
	}
}

public static class SystemTrayItemStateExtensions
{
	public static string GetServiceName(this SystemTrayItemState itemState) => itemState.StatusNotifierItemDescription.ServiceName;
}

public class AddBulkTrayItemsAction
{
	public IEnumerable<SystemTrayItemState> Items { get; init; }
}

public class AddTrayItemAction
{
	public SystemTrayItemState ItemState { get; init; }
}

public class RemoveTrayItemAction
{
	public string ServiceName { get; init; }
}

public class UpdateMenuLayoutAction
{
	public string ServiceName { get; init; }
	public DbusMenuItem RootMenuItem { get; init; }
}

public class ActivateApplicationAction
{
	public DbusObjectDescription DbusObjectDescription { get; init; }
	public int X { get; init; }
	public int Y { get; init; }
}

public class ActivateMenuItemAction
{
	public DbusObjectDescription DbusObjectDescription { get; init; }
	public int MenuItemId { get; init; }
}

public class UpdateStatusNotifierItemPropertiesAction
{
	public string ServiceName { get; init; }
	public StatusNotifierItemProperties Properties { get; init; }
}

public static class SystemTrayItemStateReducers
{
	public static readonly FeatureReducerCollection Reducers = new()
	{
		FeatureReducer.Build(new SystemTrayState())
			.On<UpdateStatusNotifierItemPropertiesAction>((s, a) => s.Items.TryGetValue(a.ServiceName, out var currentItem)
				? new() { Items = s.Items.SetItem(a.ServiceName, new SystemTrayItemState(currentItem) { Properties = a.Properties }) }
				: s)
			.On<UpdateMenuLayoutAction>((s, a) => s.Items.TryGetValue(a.ServiceName, out var currentItem)
				? new() { Items = s.Items.SetItem(a.ServiceName, new SystemTrayItemState(currentItem) { RootMenuItem = a.RootMenuItem }) }
				: s)
			.On<AddBulkTrayItemsAction>((s, a) =>
			{
				var newItemList = new LinkedList<SystemTrayItemState>();

				foreach (var item in a.Items)
				{
					if (!s.Items.ContainsKey(item.GetServiceName()))
					{
						newItemList.AddLast(item);
					}
				}

				// Add existing items too

				return new SystemTrayState()
				{
					Items = newItemList
						.DistinctBy(i => i.StatusNotifierItemDescription.ServiceName)
						.ToImmutableDictionary(i => i.StatusNotifierItemDescription.ServiceName, i => i)
				};
			})
			.On<AddTrayItemAction>((s, a) => !s.Items.ContainsKey(a.ItemState.GetServiceName())
				? new() { Items = s.Items.Add(a.ItemState.GetServiceName(), a.ItemState) }
				: s)
			.On<RemoveTrayItemAction>((s, a) => s.Items.ContainsKey(a.ServiceName)
				? new() { Items = s.Items.Remove(a.ServiceName) }
				: s)
	};
}

public class SystemTrayItemStateEffects(DBusSystemTrayService systemTrayService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		CreateEffect<ActivateApplicationAction>(async action => await systemTrayService.ActivateSystemTrayItemAsync(action.DbusObjectDescription, action.X, action.Y)),
		CreateEffect<ActivateMenuItemAction>(async action => await systemTrayService.ClickedItem(action.DbusObjectDescription, action.MenuItemId))
	};
}
