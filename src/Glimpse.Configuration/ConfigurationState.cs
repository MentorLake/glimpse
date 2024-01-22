using System.Collections.Immutable;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.Configuration;

internal class UpdateConfigurationAction
{
	public ConfigurationFile ConfigurationFile { get; set; }
}

public static class ConfigurationSelectors
{
	public static readonly ISelector<ConfigurationFile> Configuration = CreateFeature<ConfigurationFile>();
	public static readonly ISelector<string> VolumeCommand = Create(Configuration, s => s.VolumeCommand);
	public static readonly ISelector<string> TaskManagerCommand = Create(Configuration, s => s.TaskManagerCommand);

}

internal class AllReducers
{
	public static readonly FeatureReducerCollection Reducers = new()
	{
		FeatureReducer.Build(new ConfigurationFile())
			.On<UpdateConfigurationAction>((s, a) => a.ConfigurationFile)
	};
}
