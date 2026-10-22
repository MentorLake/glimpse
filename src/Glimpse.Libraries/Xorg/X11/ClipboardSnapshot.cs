namespace Glimpse.Libraries.Xorg.X11;

internal record ClipboardSnapshot(string Text, ulong Timestamp)
{
	public static ClipboardSnapshot Empty { get; } = new("", 0);

	public bool IsEmpty => Text.Length == 0;
}
