namespace Glimpse.Services.Clipboard;

public record ClipboardEntry
{
	public const int MaxTextLength = 50_000;

	public Guid Id { get; init; } = Guid.NewGuid();
	public string Text { get; init; } = "";
	public DateTime CapturedAt { get; init; } = DateTime.UtcNow;
	public bool IsPinned { get; init; }

	public static ClipboardEntry? Create(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}

		var trimmed = text.Length > MaxTextLength ? text[..MaxTextLength] : text;

		return new ClipboardEntry { Text = trimmed };
	}
}
