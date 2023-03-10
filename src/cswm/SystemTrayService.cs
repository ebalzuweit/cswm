using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using cswm.Events;
using cswm.WindowManagement;
using cswm.WindowManagement.Tracking;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace cswm;

public class SystemTrayService
{
    private readonly ILogger _logger;
	private readonly MessageBus _bus;
	private readonly WindowTrackingService _windowTracker;
	private NotifyIcon _notifyIcon;
    private Thread? _thread;
    private Version? _version;

	public SystemTrayService(ILogger<SystemTrayService> logger, MessageBus bus, WindowTrackingService windowTrackingService)
	{
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		_windowTracker = windowTrackingService ?? throw new ArgumentNullException(nameof(windowTrackingService));
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
        _logger.LogDebug("Updating context menu");
        var contextMenu = _notifyIcon.ContextMenuStrip;
        contextMenu.Items.Clear();
        contextMenu.Items.Add(new ToolStripMenuItem() { Text = $"cswm {_version}", Enabled = false });
        contextMenu.Items.Add(new ToolStripSeparator());
        if (_windowTracker.Windows.Any())
        {
            foreach (var window in _windowTracker.Windows)
            {
                contextMenu.Items.Add(new ToolStripMenuItem() { Text = window.Caption.Truncate(40), Enabled = true });
            }
            contextMenu.Items.Add(new ToolStripSeparator());
        }
        contextMenu.Items.Add("Close", null, Close_OnClick);

        e.Cancel = false;
    }
}