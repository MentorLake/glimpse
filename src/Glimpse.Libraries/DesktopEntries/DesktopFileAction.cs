namespace Glimpse.Libraries.DesktopEntries;

public class DesktopFileAction
{
	protected bool Equals(DesktopFileAction other) => Id == other.Id && ActionName == other.ActionName && DesktopFilePath == other.DesktopFilePath;

	public override bool Equals(object obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((DesktopFileAction)obj);
	}

	public override int GetHashCode() => HashCode.Combine(Id, ActionName, DesktopFilePath);

	public string Id { get; set; }
	public string ActionName { get; set; }
	public string DesktopFilePath { get; set; }
}
