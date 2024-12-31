using cswm.Core.Services;
using cswm.Core.Test.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
		// HACK: Re-register as Singleton for mocks
		services.RemoveAll<WindowRegistry>()
			.AddSingleton<WindowRegistry>();

		// Mocks are registered as singleton, so that tests can access the same service as the SUT.
		var eventHook = new MockWindowEventHook();
		services.AddSingleton<IWindowEventHook>(eventHook)
			// Register the mock directly for DI in test classes.
			.AddSingleton<MockWindowEventHook>(eventHook)
			.AddSingleton<IWindowController, MockWindowController>();

		return services;
	}
}