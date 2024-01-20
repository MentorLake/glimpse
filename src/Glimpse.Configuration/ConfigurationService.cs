using MentorLake.Redux;

namespace Glimpse.Configuration;

public class ConfigurationService(ReduxStore store)
{
	public void UpdateConfiguration(ConfigurationFile newConfiguration)
	{
		store.Dispatch(new UpdateConfigurationAction() { ConfigurationFile = newConfiguration });
	}

	public IObservable<ConfigurationFile> ConfigurationUpdated => store.Select(ConfigurationSelectors.Configuration);
}
