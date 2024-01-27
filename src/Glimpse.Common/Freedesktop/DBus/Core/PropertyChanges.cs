namespace Glimpse.Common.Freedesktop.DBus.Core;

public record PropertyChanges<TProperties>(TProperties Properties, string[] Invalidated, string[] Changed)
{
	public bool HasChanged(string property) => Array.IndexOf(Changed, property) != -1;
	public bool IsInvalidated(string property) => Array.IndexOf(Invalidated, property) != -1;
}
