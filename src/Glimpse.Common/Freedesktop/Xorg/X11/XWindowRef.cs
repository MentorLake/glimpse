namespace Glimpse.Common.Freedesktop.Xorg.X11;

internal struct XWindowRef : IWindowRef, IEquatable<XWindowRef>
{
	public ulong Id => Window;
	public ulong Window { get; init; }
	public ulong Display { get; init; }

	public bool Equals(XWindowRef other)
	{
		return Id == other.Id;
	}
}
