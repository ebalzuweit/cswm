using System;
using cswm.Events;
using cswm.WindowManagement;
using cswm.WindowManagement.Arrangement;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cswm;

internal static class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		using var host = BuildHost(args);
		using var scope = host.Services.CreateScope();
		var startup = scope.ServiceProvider.GetRequiredService<Startup>();

		host.Start();
		startup.Start();
	}

	private static IHost BuildHost(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);
		builder.ConfigureServices((_, services) =>
		{
			services.AddOptions<WindowManagementOptions>()
				.BindConfiguration(nameof(WindowManagementOptions))
				.ValidateOnStart();

			services.AddSingleton<MessageBus>();
			services.AddSingleton<SystemTrayService>();
			services.AddSingleton<WinHookService>();
			services.AddSingleton<WindowManagementService>();
			services.AddSingleton<Startup>();

			services.AddTransient<WindowTrackingService>();
			services.AddTransient<IWindowTrackingStrategy, DefaultWindowTrackingStrategy>();
			services.AddTransient<IArrangementStrategy, SplitArrangementStrategy>();
		});
		builder.ConfigureLogging(logging =>
		{
			logging.ClearProviders();
#if DEBUG
			logging.AddDebug();
			logging.AddConsole();
#endif
		});
		return builder.Build();
	}
}
