using cswm.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace cswm.App;

/// <summary>
/// Build services and start the application.
/// </summary>
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
		builder.ConfigureServices((_, services) => services.AddCswmServices());
		builder.ConfigureLogging(logging =>
		{
			logging.ClearProviders();
#if DEBUG
			logging.AddDebug();
#endif
		});
		return builder.Build();
	}
}
