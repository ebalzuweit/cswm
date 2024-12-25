using System;
using cswm.Core;
using cswm.Core.Management;
using cswm.Windows.Management;
using Microsoft.Extensions.DependencyInjection;

namespace cswm.Windows;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddCswmWindowsServices(this IServiceCollection services)
		=> services.AddCswmCoreServices()
			.AddTransient<IWindowController, WindowsWindowController>()
			// Only register a single Windows event hook instance
			.AddSingleton<IWindowEventHook, WindowsWindowEventHook>();
}
