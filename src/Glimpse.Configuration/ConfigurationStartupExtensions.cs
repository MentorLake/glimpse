using System.Reactive.Linq;
using System.Text.Json;
using Glimpse.Common.Microsoft.Extensions;
using MentorLake.Redux;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glimpse.Configuration;

public static class ConfigurationStartupExtensions
{
	public static void AddGlimpseConfiguration(this IHostApplicationBuilder builder)
	{
		builder.Services.AddInstance(AllReducers.Reducers);
		builder.Services.AddSingleton<ConfigurationService>();
	}

	public static Task UseGlimpseConfiguration(this IHost host)
	{
		var store = host.Services.GetRequiredService<ReduxStore>();
		var dataDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "glimpse");
		var configFile = Path.Join(dataDirectory, "config.json");

		if (!Directory.Exists(dataDirectory))
		{
			Directory.CreateDirectory(dataDirectory);
		}

		if (!File.Exists(configFile))
		{
			File.WriteAllText(configFile, JsonSerializer.Serialize(new ConfigurationFile(), typeof(ConfigurationFile), ConfigurationSerializationContext.Instance));
		}

		// Add file watcher
		var config = (ConfigurationFile) JsonSerializer.Deserialize(
			File.ReadAllText(configFile),
			typeof(ConfigurationFile),
			ConfigurationSerializationContext.Instance);

		store.Dispatch(new UpdateConfigurationAction() { ConfigurationFile = config });

		store.Select(ConfigurationSelectors.Configuration).Skip(1).Subscribe(f =>
		{
			Console.WriteLine("Writing");
			File.WriteAllText(configFile, JsonSerializer.Serialize(f, typeof(ConfigurationFile), ConfigurationSerializationContext.Instance));
		});

		return Task.CompletedTask;
	}
}
