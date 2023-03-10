using System;
using System.Text;
using cswm.Events;
using cswm.Logging;
using cswm.WinApi;
using cswm.WindowManagement;
using cswm.WindowManagement.Arrangement;
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

#if DEBUG
		var debugLogger = scope.ServiceProvider.GetRequiredService<WinEventLogger>();
		debugLogger.AddFilter(@event =>
		{
			var window = new Window(@event.hWnd);
			if (window.ClassName.Contains("RCLIENT", StringComparison.OrdinalIgnoreCase))
				return true;
			if (window.ClassName.Contains("Chrome_WidgetWin_1", StringComparison.OrdinalIgnoreCase))
				return true;
			return false;
		});
		debugLogger.AddFormat(@event =>
		{
			var window = new Window(@event.hWnd);
			var windowStyles = (long)User32.GetWindowLongPtr(@event.hWnd, WindowLongFlags.GWL_STYLE);
			var windowExStyles = (long)User32.GetWindowLongPtr(@event.hWnd, WindowLongFlags.GWL_EXSTYLE);
			var sb = new StringBuilder(window.ToString());
			sb.AppendFormat("\nGWL_STYLE: {0}", windowStyles);
			sb.AppendFormat("\tGWL_EX_STYLE: {0}", windowExStyles);
			foreach (var style in Enum.GetValues<WindowStyle>())
			{
				var styleName = Enum.GetName<WindowStyle>(style);
				var hasStyle = ((long)style & windowStyles) == 0;
				sb.AppendFormat("\n{0}: {1}", styleName, hasStyle);
			}
			return sb.ToString();
		});
#endif

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
			services.AddTransient<IArrangementStrategy, SplitArrangementStrategy>();

#if DEBUG
			services.AddTransient<WinEventLogger>();
#endif
		});
		builder.ConfigureLogging(logging =>
		{
			logging.ClearProviders();
#if DEBUG
			logging.AddConsole();
#endif
		});
		return builder.Build();
	}
}
