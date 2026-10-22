using System.Reactive;
using System.Reactive.Concurrency;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Libraries.Xorg;

namespace Glimpse.Services.Clipboard;

public class ClipboardMonitorService(ILogger<ClipboardMonitorService> logger, ReduxStore store, IDisplayServer displayServer)
{
	private bool _monitoringEnabled;

	public async Task InitializeAsync(string clipboardHistoryFilePath)
	{
		await LoadHistoryFromFileAsync(clipboardHistoryFilePath);

		store
			.Select(ClipboardSelectors.History)
			.ConflateLatest(TaskPoolScheduler.Default)
			.Subscribe(history => PersistHistory(history, clipboardHistoryFilePath));

		store
			.Select(ClipboardSelectors.Configuration)
			.ConflateLatest(TaskPoolScheduler.Default)
			.Subscribe(config =>
			{
				if (config.Enabled && !_monitoringEnabled)
				{
					_monitoringEnabled = true;
					StartMonitoring();
				}
			});
	}

	private void StartMonitoring()
	{
		displayServer
			.ClipboardTextChanged
			.SubscribeDebug(OnClipboardTextRead);
	}

	private void OnClipboardTextRead(string text)
	{
		store.Dispatch(new CaptureClipboardItemAction(text));
	}

	private async Task LoadHistoryFromFileAsync(string clipboardHistoryFilePath)
	{
		if (!File.Exists(clipboardHistoryFilePath))
		{
			return;
		}

		try
		{
			var historyJson = await File.ReadAllTextAsync(clipboardHistoryFilePath);

			try
			{
				var history = JsonSerializer.Deserialize(historyJson, ClipboardSerializationContext.Instance.ClipboardHistory);

				if (history != null)
				{
					store.Dispatch(new LoadClipboardHistoryAction(history));
				}
			}
			catch (JsonException)
			{
			}
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to load clipboard history");
		}
	}

	private void PersistHistory(ClipboardHistory history, string clipboardHistoryFilePath)
	{
		try
		{
			var directory = Path.GetDirectoryName(clipboardHistoryFilePath);
			if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
			File.WriteAllText(clipboardHistoryFilePath, JsonSerializer.Serialize(history, typeof(ClipboardHistory), ClipboardSerializationContext.Instance));
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to persist clipboard history");
		}
	}
}
