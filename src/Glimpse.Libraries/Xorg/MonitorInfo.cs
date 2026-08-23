using System.Drawing;

namespace Glimpse.Libraries.Xorg;

public record MonitorInfo
{
	public long Id { get; init; }
	public string Name { get; init; } = "";
	public Rectangle Geometry { get; init; }
	public Rectangle WorkArea { get; init; }
	public bool IsPrimary { get; init; }
}
