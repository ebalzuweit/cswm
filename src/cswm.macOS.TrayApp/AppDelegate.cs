using AppKit;
using cswm.Core;
using cswm.macOS.External;
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
        // Verify we have the necessary permissions to list & manage other application windows
        CheckAndRequestAccessibilityPermissions();
        
        // Setup DI host
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => services
                .AddCswmCoreServices()
                .AddCswmMacOsServices()
                .AddTransient<CswmStatusBarItem>()
            )
            .Build();
        host.Start();

        // Setup status bar icon
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

    private void CheckAndRequestAccessibilityPermissions()
    {
        // Check required accessibility permissions have been granted
        var trusted = ApplicationServices.AXIsProcessTrusted();
        if (trusted == false)
        {
            // Request permission and close
            NSApplication.SharedApplication.InvokeOnMainThread(RequestAccessibilityPermissions);
        }
        
    }
    
    private void RequestAccessibilityPermissions()
    {
        using var alert = new NSAlert();
        alert.AlertStyle = NSAlertStyle.Informational;
        alert.MessageText = "Accessibility Permissions Required";
        alert.InformativeText = "This app needs accessibility permissions to manage windows. Please enable in System Settings > Privacy & Security > Privacy > Accessibility, then restart the application.";
        alert.AddButton("OK");
        alert.AddButton("Cancel");
        
        // Alert will not show if we're running in the background
        NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
        
        var response = alert.RunModal();
        
        // Reset ActivationPolicy
        NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Accessory;

        if (response == (long)NSAlertButtonReturn.First)
        {
            // OK - Open the accessibility menu
            NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl("x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility"));
        }
        NSApplication.SharedApplication.Terminate(this);
    }
}
