using System.Collections.Immutable;
using System.Reactive.Linq;
using Glimpse.Common.Freedesktop.DBus;
using MentorLake.Redux.Effects;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.SystemTray;

public record SystemTrayState
{
	public ImmutableDictionary<string, SystemTrayItemState> Items { get; init; } = ImmutableDictionary<string, SystemTrayItemState>.Empty;
	public SystemTrayConfiguration Configuration { get; init; } = SystemTrayConfiguration.Empty;
}

public record SystemTrayItemState
{
	public StatusNotifierItemProperties Properties { get; init; }
	public DbusObjectDescription StatusNotifierItemDescription { get; init; }
	public DbusObjectDescription DbusMenuDescription { get; init; }
	public DbusMenuItem RootMenuItem { get; init; }
	public string GetServiceName() => StatusNotifierItemDescription.ServiceName;
}

public static class SystemTraySelectors
{
	private static readonly ISelector<SystemTrayState> s_systemTrayState = SelectorFactory.CreateFeature<SystemTrayState>();
	private static readonly ISelector<SystemTrayConfiguration> s_configuration = SelectorFactory.Create(s_systemTrayState, s => s.Configuration);
	public static readonly ISelector<string> VolumeCommand = SelectorFactory.Create(s_configuration, s => s.VolumeCommand);
}

public record UpdateSystemTrayConfiguration(SystemTrayConfiguration Config);
public record AddBulkTrayItemsAction(IEnumerable<SystemTrayItemState> Items);
public record AddTrayItemAction(SystemTrayItemState ItemState);
public record RemoveTrayItemAction(string ServiceName);
public record UpdateMenuLayoutAction(string ServiceName, DbusMenuItem RootMenuItem);

public record ActivateApplicationAction
{
	public DbusObjectDescription DbusObjectDescription { get; init; }
	public int X { get; init; }
	public int Y { get; init; }
}

public record ActivateMenuItemAction
{
	public DbusObjectDescription DbusObjectDescription { get; init; }
	public int MenuItemId { get; init; }
}

public record UpdateStatusNotifierItemPropertiesAction
{
	public string ServiceName { get; init; }
	public StatusNotifierItemProperties Properties { get; init; }
}

public record SystemTrayItemStateReducers : IReducerFactory
{
	public FeatureReducerCollection Create() =>
	[
		FeatureReducer.Build(new SystemTrayState())
			.On<UpdateSystemTrayConfiguration>((s, a) => s with { Configuration = a.Config })
			.On<UpdateStatusNotifierItemPropertiesAction>((s, a) => s.Items.TryGetValue(a.ServiceName, out var currentItem)
				? s with { Items = s.Items.SetItem(a.ServiceName, currentItem with { Properties = a.Properties }) }
				: s)
			.On<UpdateMenuLayoutAction>((s, a) => s.Items.TryGetValue(a.ServiceName, out var currentItem)
				? s with { Items = s.Items.SetItem(a.ServiceName, currentItem with { RootMenuItem = a.RootMenuItem }) }
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

				return s with
				{
					Items = newItemList
						.DistinctBy(i => i.StatusNotifierItemDescription.ServiceName)
						.ToImmutableDictionary(i => i.StatusNotifierItemDescription.ServiceName, i => i)
				};
			})
			.On<AddTrayItemAction>((s, a) => !s.Items.ContainsKey(a.ItemState.GetServiceName())
				? s with { Items = s.Items.Add(a.ItemState.GetServiceName(), a.ItemState) }
				: s)
			.On<RemoveTrayItemAction>((s, a) => s.Items.ContainsKey(a.ServiceName)
				? s with { Items = s.Items.Remove(a.ServiceName) }
				: s)
	];
}

public class SystemTrayItemStateEffects(DBusSystemTrayService systemTrayService) : IEffectsFactory
{
	public IEnumerable<Effect> Create() => new[]
	{
		EffectsFactory.Create(actions => actions
			.OfType<ActivateApplicationAction>()
			.Select(action => Observable.FromAsync(() => systemTrayService.ActivateSystemTrayItemAsync(action.DbusObjectDescription, action.X, action.Y)))
			.Concat()),
		EffectsFactory.Create(actions => actions
			.OfType<ActivateMenuItemAction>()
			.Select(action => Observable.FromAsync(() => systemTrayService.ClickedItem(action.DbusObjectDescription, action.MenuItemId)))
			.Concat())
	};
}
