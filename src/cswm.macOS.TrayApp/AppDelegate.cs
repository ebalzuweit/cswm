using AppKit;
using cswm.Core;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace cswm.macOS.TrayApp;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private IHost? host;
    private CswmStatusBarItem? statusBarItem;

    public override void DidFinishLaunching(NSNotification notification)
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => services
                .AddCswmCoreServices()
                .AddCswmMacOsServices()
                .AddTransient<CswmStatusBarItem>()
            )
            .Build();

        host.Start();

        statusBarItem = host.Services.GetRequiredService<CswmStatusBarItem>();
        statusBarItem.AddToStatusBar();
    }

    public override void WillTerminate(NSNotification notification)
    {
        statusBarItem = null;
        if (host is not null)
        {
            host?.StopAsync().Wait();
            host?.Dispose();
            host = null;
        }
    }
}
