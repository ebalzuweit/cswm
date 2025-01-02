using cswm.Core.Services;
using cswm.Core.Test.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace cswm.Core.Test;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services
			.AddCswmCoreServices()
			.AddCswmCoreMockServices()
			.AddLogging(x => x.AddXunitOutput());
	}
}

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCswmCoreMockServices(this IServiceCollection services)
	{
		// Mocks are registered as singleton, so that tests can access the same service as the SUT.
		services.AddSingleton<IWindowEventHook, MockWindowEventHook>()
			.AddSingleton<IWindowController, MockWindowController>();

		return services;
	}
}