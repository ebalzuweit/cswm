using System;
using System.Windows.Forms;
using cswm.Events;
using cswm.WinApi;
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

        startup.Start();
        Application.Run();
    }

    private static IHost BuildHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<MessageBus>();
                services.AddSingleton<SystemTrayService>();
                services.AddSingleton<WinHookService>();
                services.AddSingleton<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();
    }
}
