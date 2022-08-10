using System;
using cswm.Events;
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
        // TODO: application guid mutex

        using var host = BuildHost(args);
        using var scope = host.Services.CreateScope();
        var startup = scope.ServiceProvider.GetRequiredService<Startup>();

        host.Start();
        startup.Start();
    }

    private static IHost BuildHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<MessageBus>();
                services.AddSingleton<SystemTrayService>();
                services.AddSingleton<WinHookService>();
                services.AddSingleton<WindowManagementService>();
                services.AddSingleton<Startup>();

                services.AddTransient<WindowTrackingService>();
                services.AddTransient<IArrangementStrategy, SplitArrangementStrategy>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
#if DEBUG
                logging.AddConsole();
#endif
            })
            .Build();
    }
}
