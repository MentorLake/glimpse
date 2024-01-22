using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using MentorLake.Redux;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Configuration;

public class ConfigurationService(ReduxStore store)
{
	private string _filePath;
	private Dictionary<string, JsonElement> _sections = new();
	private IObservable<Unit> _fileChangedObs;

	public void Load(string filePath)
	{
		var watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath)!, Path.GetFileName(filePath));
		watcher.EnableRaisingEvents = true;
		watcher.IncludeSubdirectories = false;

		_filePath = filePath;

		_fileChangedObs = watcher.Events().Changed
			.Throttle(TimeSpan.FromMilliseconds(250), new EventLoopScheduler())
			.Do(_ => LoadFile())
			.Select(_ => Unit.Default)
			.RetryWhen(errorObs => errorObs
				.Do(Console.WriteLine)
				.Select(_ => Observable.Timer(TimeSpan.FromSeconds(1))))
			.Publish()
			.AutoConnect();

		LoadFile();

		var config = (ConfigurationFile) JsonSerializer.Deserialize(
			File.ReadAllText(filePath),
			typeof(ConfigurationFile),
			ConfigurationSerializationContext.Instance);

		store.Dispatch(new UpdateConfigurationAction() { ConfigurationFile = config });
	}

	private void LoadFile()
	{
		_sections = new Dictionary<string, JsonElement>();

		var jsonDocument = JsonDocument.Parse(File.ReadAllText(_filePath));

		foreach (var prop in jsonDocument.RootElement.EnumerateObject())
		{
			_sections.Add(prop.Name, prop.Value);
		}
	}

	public IObservable<JsonElement> ObserveChange(string key)
	{
		var changeObs = _fileChangedObs.Select(_ => _sections[key]);

		if (_sections.TryGetValue(key, out var currentValue))
		{
			return Observable.Return(currentValue).Concat(changeObs);
		}

		return changeObs;
	}

	public void AddIfNotExists(string key, JsonElement element)
	{
		if (_sections.TryAdd(key, element))
		{
			File.WriteAllText(_filePath, JsonSerializer.Serialize(_sections, _sections.GetType(), ConfigurationSerializationContext.Instance));
		}
	}

	public void Upsert(string key, JsonElement element)
	{
		if (!_sections.TryAdd(key, element))
		{
			_sections[key] = element;
		}

		File.WriteAllText(_filePath, JsonSerializer.Serialize(_sections, _sections.GetType(), ConfigurationSerializationContext.Instance));
	}

	public bool ContainsKey(string key)
	{
		return _sections.ContainsKey(key);
	}
}
