using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Libraries.Configuration;

public class ConfigurationService(ILogger<ConfigurationService> logger)
{
	private Dictionary<string, JsonObject> _sections = new();
	private IObservable<Unit> _fileChangedObs;
	private string _directory;
	private string _fileName;
	private string _fullPath;

	public void Initialize(string configFilePath)
	{
		_directory = Path.GetDirectoryName(configFilePath);
		_fileName = Path.GetFileName(configFilePath);
		_fullPath = configFilePath;
		EnsureDataDirectoryExists();

		var watcher = new FileSystemWatcher(_directory, _fileName);
		watcher.EnableRaisingEvents = true;
		watcher.IncludeSubdirectories = false;

		_fileChangedObs = watcher.Events().Changed
			.Throttle(TimeSpan.FromMilliseconds(250), new EventLoopScheduler())
			.Do(_ => LoadFile())
			.Select(_ => Unit.Default)
			.RetryWhen(errorObs => errorObs
				.Do(e => logger.LogError(e.ToString()))
				.Select(_ => Observable.Timer(TimeSpan.FromSeconds(1))))
			.Publish()
			.AutoConnect();

		LoadFile();
	}

	private void EnsureDataDirectoryExists()
	{
		if (!Directory.Exists(_directory))
		{
			Directory.CreateDirectory(_directory);
		}
	}

	private void SaveFile()
	{
		EnsureDataDirectoryExists();
		File.WriteAllText(_fullPath, JsonSerializer.Serialize(_sections, _sections.GetType(), ConfigurationSerializationContext.Instance));
	}

	private void LoadFile()
	{
		if (!File.Exists(_fullPath))
		{
			SaveFile();
		}

		_sections = new Dictionary<string, JsonObject>();

		var jsonRoot = JsonNode.Parse(File.ReadAllText(_fullPath));

		if (jsonRoot?.GetValueKind() == JsonValueKind.Object)
		{
			foreach (var prop in jsonRoot.AsObject().Where(o => o.Value?.GetValueKind() == JsonValueKind.Object))
			{
				_sections.Add(prop.Key, prop.Value.AsObject());
			}
		}
	}

	public IObservable<JsonObject> ObserveChange(string key)
	{
		var changeObs = _fileChangedObs.Select(_ => _sections[key]);

		if (_sections.TryGetValue(key, out var currentValue))
		{
			return Observable.Return(currentValue).Concat(changeObs);
		}

		return changeObs;
	}

	public void AddIfNotExists(string key, JsonObject element)
	{
		if (_sections.TryAdd(key, element))
		{
			SaveFile();
		}
	}

	public void Upsert(string key, JsonObject element)
	{
		if (!_sections.TryAdd(key, element))
		{
			_sections[key] = element;
		}

		SaveFile();
	}
}
