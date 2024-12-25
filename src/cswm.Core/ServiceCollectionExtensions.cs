using cswm.Core.Layout;
using cswm.Core.Layout.Engines;
using cswm.Core.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace cswm.Core;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCswmCoreServices(this IServiceCollection services)
		=> services
			.AddTransient<ILayoutEngine, BspTilingLayoutEngine>()
			.AddTransient<WindowRegistry>()
			.AddTransient<WindowManager>();
}
