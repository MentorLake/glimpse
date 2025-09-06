using System.Collections.Immutable;
using MentorLake.GdkPixbuf;
using MentorLake.Redux;

namespace Glimpse.Libraries.Xorg;

public record WindowProperties : IKeyed<ulong>
{
	public ulong Id => WindowRef.Id;
	public IWindowRef WindowRef { get; set; }
	public string Title { get; init; }
	public string IconName { get; init; }
	public ImmutableList<GdkPixbufHandle> Icons { get; init; } = ImmutableList<GdkPixbufHandle>.Empty;
	public string ClassHintName { get; init; }
	public string ClassHintClass { get; set; }
	public bool DemandsAttention { get; set; }
	public AllowedWindowActions[] AllowActions { get; set; }
	public DateTime CreationDate { get; set; }
	public GdkPixbufHandle DefaultScreenshot { get; set; }

	public virtual bool Equals(WindowProperties other) => ReferenceEquals(this, other);
}
