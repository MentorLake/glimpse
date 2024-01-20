using Microsoft.Extensions.DependencyInjection;

namespace Glimpse.Common.Microsoft.Extensions;

public static class ServiceCollectionExtensions
{
	public static void AddInstance<T>(this IServiceCollection services, T instance)
	{
		services.Add(new ServiceDescriptor(typeof(T), instance));
	}
}
