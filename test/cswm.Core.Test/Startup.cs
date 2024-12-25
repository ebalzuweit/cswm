using cswm.Core.Management;
using cswm.Core.Test.Management;
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
		// Register as Singleton for tests
		var eventHook = new MockWindowEventHook();
		services.AddSingleton<IWindowEventHook>(eventHook);
		services.AddSingleton(eventHook);

		services.AddTransient<IWindowController, MockWindowController>();

		return services;
	}
}