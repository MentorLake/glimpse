using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.Common.Configuration;

public class ConfigurationService
{
	private Dictionary<string, JsonObject> _sections = new();
	private IObservable<Unit> _fileChangedObs;
	private static readonly string s_fileName = "config.json";
	private static readonly string s_dataDirectoryPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "glimpse");
	public static readonly string FilePath = Path.Join(s_dataDirectoryPath, s_fileName);

	public void Initialize()
	{
		EnsureDataDirectoryExists();

		var watcher = new FileSystemWatcher(s_dataDirectoryPath, s_fileName);
		watcher.EnableRaisingEvents = true;
		watcher.IncludeSubdirectories = false;

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
	}

	private void EnsureDataDirectoryExists()
	{
		if (!Directory.Exists(s_dataDirectoryPath))
		{
			Directory.CreateDirectory(s_dataDirectoryPath);
		}
	}

	private void SaveFile()
	{
		EnsureDataDirectoryExists();
		File.WriteAllText(FilePath, JsonSerializer.Serialize(_sections, _sections.GetType(), ConfigurationSerializationContext.Instance));
	}

	private void LoadFile()
	{
		_sections = new Dictionary<string, JsonObject>();

		var jsonRoot = JsonNode.Parse(File.ReadAllText(FilePath));

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
