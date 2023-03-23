using cswm.Events;
using cswm.WindowManagement;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace cswm;

public class SystemTrayService
{
    private readonly ILogger _logger;
    private readonly MessageBus _bus;
    private readonly WindowManagementService _windowManager;
    private NotifyIcon? _notifyIcon;
    private Thread? _thread;
    private Version? _version;

    public SystemTrayService(ILogger<SystemTrayService> logger, MessageBus bus, WindowManagementService windowMgmtService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _windowManager = windowMgmtService ?? throw new ArgumentNullException(nameof(windowMgmtService));
    }

    public void AddToSystemTray()
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

    public void RemoveFromSystemTray()
    {
        _logger.LogInformation("Removing notification icon from system tray...");

        if (_notifyIcon is not null)
            _notifyIcon!.Visible = false;
        _notifyIcon?.Dispose();

        _thread = null;
    }

    private void SystemTrayApp()
    {
        _notifyIcon = BuildNotificationIcon();

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
        const string iconResourceName = "cswm.icon.ico";

        NotifyIcon icon;
        var assembly = Assembly.GetEntryAssembly()!;
        _version = assembly.GetName().Version;
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
        icon.DoubleClick += NotifyIcon_OnDoubleClick;

        return icon;
    }

    private void ContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        if (_notifyIcon is null)
            throw new InvalidOperationException("Notification icon has not been built, cannot open context menu.");

        _logger.LogDebug("Updating context menu");

        var windowItems = _windowManager.Windows.Select(w => WindowMenu(w)).ToArray();
        var contextMenu = _notifyIcon.ContextMenuStrip;
        contextMenu.Items.Clear();
        contextMenu.Items.Add(AboutMenu());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(WindowListMenu(windowItems));
        contextMenu.Items.Add("Refresh", null, Refresh_OnClick);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Close", null, Close_OnClick);
        e.Cancel = false;

        const string AboutUrl = "https://github.com/ebalzuweit/cswm";
        ToolStripMenuItem AboutMenu() => new($"cswm v{_version}", null, (s, e) => Process.Start(new ProcessStartInfo(AboutUrl) { UseShellExecute = true }));
        ToolStripMenuItem WindowListMenu(ToolStripMenuItem[] windowItems) => new("Tracked windows", null, windowItems);
        ToolStripMenuItem WindowMenu(Window window)
        {
            var managed = _windowManager.IsWindowManaged(window);
            return new(window.Caption.Truncate(40), null, (s, e) => _windowManager.SetWindowManaged(window, !managed))
            {
                Checked = managed,
            };
        }
    }
}
