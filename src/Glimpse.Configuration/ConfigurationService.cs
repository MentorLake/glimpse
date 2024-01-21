using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
			.Delay(TimeSpan.FromMilliseconds(250))
			.Do(_ => LoadFile())
			.Select(_ => Unit.Default)
			.RetryWhen(errorObs => errorObs
				.Do(Console.WriteLine)
				.Select(_ => Observable.Timer(TimeSpan.FromSeconds(1))));

		LoadFile();

		var config = (ConfigurationFile) JsonSerializer.Deserialize(
			File.ReadAllText(filePath),
			typeof(ConfigurationFile),
			ConfigurationSerializationContext.Instance);

		store.Dispatch(new UpdateConfigurationAction() { ConfigurationFile = config });

		store.Select(ConfigurationSelectors.Configuration).Skip(1).Subscribe(f =>
		{
			Console.WriteLine("Writing");
			File.WriteAllText(filePath, JsonSerializer.Serialize(f, typeof(ConfigurationFile), ConfigurationSerializationContext.Instance));
		});
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

	public IObservable<T> ObserveChange<T>(string key, JsonSerializerContext serializationContext)
	{
		var changeObs = _fileChangedObs.Select(_ => Get<T>(key, serializationContext));

		if (TryGet<T>(key, serializationContext, out var currentValue))
		{
			return Observable.Return(currentValue).Concat(changeObs);
		}

		return changeObs;
	}

	public void Upsert<T>(string key, T val, JsonSerializerContext serializationContext)
	{
		var element = JsonSerializer.SerializeToElement(val, typeof(T), serializationContext);

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

	private T Get<T>(string key, JsonSerializerContext serializationContext)
	{
		return (T) _sections[key].Deserialize(typeof(T), serializationContext);
	}

	private bool TryGet<T>(string key, JsonSerializerContext serializationContext, out T result)
	{
		if (ContainsKey(key))
		{
			result = (T) _sections[key].Deserialize(typeof(T), serializationContext);
			return true;
		}

		result = default(T);
		return false;
	}

	public void UpdateConfiguration(ConfigurationFile newConfiguration)
	{
		store.Dispatch(new UpdateConfigurationAction() { ConfigurationFile = newConfiguration });
	}
}
