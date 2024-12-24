using cswm.Core.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace cswm.Core;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCswmServices(this IServiceCollection services)
	{
		services.AddTransient<WindowRegistry>();

		return services;
	}
}
