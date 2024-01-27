using Glimpse.Common.Images;
using MentorLake.Redux;

namespace Glimpse.Common.Freedesktop.Xorg;

public record WindowProperties : IKeyed<ulong>
{
	public ulong Id => WindowRef.Id;
	public IWindowRef WindowRef { get; set; }
	public string Title { get; init; }
	public string IconName { get; init; }
	public List<IGlimpseImage> Icons { get; init; }
	public string ClassHintName { get; init; }
	public string ClassHintClass { get; set; }
	public bool DemandsAttention { get; set; }
	public AllowedWindowActions[] AllowActions { get; set; }
	public uint Pid { get; set; }
	public DateTime CreationDate { get; set; }
	public IGlimpseImage DefaultScreenshot { get; set; }

	public virtual bool Equals(WindowProperties other) => ReferenceEquals(this, other);
}
