namespace Glimpse.Libraries.DBus.Core;

public static class SignalHelper
{
	public static ValueTask<IDisposable> WatchSignalAsync(DBusConnection connection, MatchRule rule, Action<Exception?> handler, bool emitOnCapturedContext = true)
		=> connection.AddMatchAsync(rule, static (_, _) => Unit.Default, (Tmds.DBus.Protocol.Notification<Unit> notification) =>
		{
			if (notification.IsCompletion)
			{
				handler(notification.Exception);
			}
		}, emitOnCapturedContext);

	public static ValueTask<IDisposable> WatchSignalAsync<T>(DBusConnection connection, MatchRule rule, MessageValueReader<T> reader, Action<Exception?, T> handler, bool emitOnCapturedContext = true)
		=> connection.AddMatchAsync(rule, reader, (Tmds.DBus.Protocol.Notification<T> notification) =>
		{
			if (notification.IsCompletion)
			{
				handler(notification.Exception, default !);
			}
			else
			{
				handler(null, notification.Value);
			}
		}, emitOnCapturedContext);

	public static ValueTask<IDisposable> WatchPropertiesChangedAsync<T>(DBusConnection connection, string destination, string path, string @interface, MessageValueReader<PropertyChanges<T>> reader, Action<Exception?, PropertyChanges<T>> handler, bool emitOnCapturedContext = true)
	{
		var rule = new MatchRule
		{
			Type = MessageType.Signal,
			Sender = destination,
			Path = path,
			Member = "PropertiesChanged",
			Interface = "org.freedesktop.DBus.Properties",
			Arg0 = @interface
		};
		return WatchSignalAsync(connection, rule, reader, handler, emitOnCapturedContext);
	}

	public static IObservable<Unit> WatchSignal(this DBusConnection connection, MatchRule matchRule)
	{
		return Observable.Create<Unit>(async obs =>
		{
			return await connection.AddMatchAsync(
				matchRule,
				static (_, _) => Unit.Default,
				(Tmds.DBus.Protocol.Notification<Unit> notification) =>
				{
					if (notification.IsCompletion)
					{
						if (notification.Exception is not null)
						{
							obs.OnError(notification.Exception);
						}
					}
					else
					{
						obs.OnNext(Unit.Default);
					}
				},
				emitOnCapturedContext: false);
		});
	}

	public static IObservable<T> WatchSignal<T>(this DBusConnection connection, MatchRule matchRule, MessageValueReader<T> reader)
	{
		return Observable.Create<T>(async obs =>
		{
			return await connection.AddMatchAsync(
				matchRule,
				reader,
				(Tmds.DBus.Protocol.Notification<T> notification) =>
				{
					if (notification.IsCompletion)
					{
						if (notification.Exception is not null)
						{
							obs.OnError(notification.Exception);
						}
					}
					else
					{
						obs.OnNext(notification.Value);
					}
				},
				emitOnCapturedContext: false);
		});
	}
}
