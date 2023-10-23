using cswm.Events;
using cswm.Tracking.Events;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace cswm.App.Services;

/// <summary>
/// Icon running in the system tray.
/// </summary>
public class SystemTrayService : IService
{
    private readonly ILogger _logger;
    private readonly MessageBus _bus;
    private readonly SystemTrayMenu _trayMenu;

    private NotifyIcon? _notifyIcon;
    private Thread? _thread;

    public SystemTrayService(
        ILogger<SystemTrayService> logger,
        MessageBus bus,
        SystemTrayMenu trayMenu)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(trayMenu);

        _logger = logger;
        _bus = bus;
        _trayMenu = trayMenu;
    }

    public void Start()
    {
        AddToSystemTray();
    }

    public void Stop()
    {
        RemoveFromSystemTray();
    }

    private void AddToSystemTray()
    {
        _logger.LogInformation("Adding notification icon to system tray...");

        if (_thread is not null)
            throw new InvalidOperationException("Another thread already exists");
        _thread = new Thread(() => SystemTrayApp())
        {
            Name = "cswmtray"
        };
        _thread.Start();
    }

    private void RemoveFromSystemTray()
    {
        _logger.LogInformation("Removing notification icon from system tray...");

        if (_notifyIcon is not null)
        {
            _notifyIcon.DoubleClick -= NotifyIcon_OnDoubleClick;
            _notifyIcon!.Visible = false;
        }
        _notifyIcon?.Dispose();

        _thread = null;
    }

    private void SystemTrayApp()
    {
        _notifyIcon = BuildNotificationIcon();
        _notifyIcon.DoubleClick += NotifyIcon_OnDoubleClick;

        // message loop
        Application.Run();
    }

    private void NotifyIcon_OnDoubleClick(object? sender, EventArgs e)
        => _bus.Publish(new ResetTrackedWindowsEvent());

    private void Close_OnClick(object? sender, EventArgs e)
        => _bus.Publish(new ExitApplicationEvent());

    private void Refresh_OnClick(object? sender, EventArgs e)
        => _bus.Publish(new ResetTrackedWindowsEvent());

    private NotifyIcon BuildNotificationIcon()
    {
        const string iconResourceName = "cswm.App.icon.ico";

        NotifyIcon icon;
        var assembly = Assembly.GetEntryAssembly()!;
        using (var stream = assembly.GetManifestResourceStream(iconResourceName))
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Opening += new CancelEventHandler(ContextMenu_Opening);
            icon = new NotifyIcon
            {
                Icon = new Icon(stream!),
                ContextMenuStrip = contextMenu,
                Text = "cswm",
                Visible = true,
            };
        }

        return icon;
    }

    private void ContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        if (_notifyIcon is null)
            throw new InvalidOperationException("Notification icon has not been built, cannot open context menu.");

        var contextMenu = _notifyIcon.ContextMenuStrip;
        contextMenu.Items.Clear();
        contextMenu.Items.AddRange(_trayMenu.BuildTrayMenu());
        e.Cancel = false;
    }
}
