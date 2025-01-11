using Microsoft.Extensions.DependencyInjection;

namespace Glimpse.Libraries.Microsoft.Extensions;

public static class ServiceCollectionExtensions
{
	public static void AddInstance<T>(this IServiceCollection services, T instance)
	{
		services.Add(new ServiceDescriptor(typeof(T), instance));
	}
}
