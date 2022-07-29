using System;
using System.Windows.Forms;
using cswm.Events;
using cswm.WinApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace cswm;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // TODO: application guid mutex

        using var host = BuildHost();
        using var scope = host.Services.CreateScope();
        var startup = scope.ServiceProvider.GetRequiredService<Startup>();

        startup.Run();
        Application.Run();
    }

    private static IHost BuildHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<MessageBus>();
                services.AddSingleton<SystemTrayService>();
                services.AddSingleton<WinHookService>();
                services.AddSingleton<Startup>();
            })
            .Build();
    }
}
