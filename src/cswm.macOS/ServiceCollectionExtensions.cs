using cswm.Core.Services;
using cswm.macOS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace cswm.macOS;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCswmMacOsServices(this IServiceCollection services)
		=> services
			.AddTransient<IWindowController, MacOsWindowController>()
			.AddTransient<IWindowEventHook, MacOsWindowEventHook>();
}
