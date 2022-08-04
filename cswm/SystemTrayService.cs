using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using cswm.Events;
using cswm.WindowManagement;

namespace cswm;

public class SystemTrayService
{
    private readonly MessageBus _bus;
    private NotifyIcon? _notifyIcon;

    public SystemTrayService(MessageBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public void AddToSystemTray()
    {
        var thread = new Thread(() => SystemTrayApp())
        {
            Name = "cswmtray"
        };
        thread.Start();
    }

    public void RemoveFromSystemTray()
    {
        if (_notifyIcon is not null)
            _notifyIcon!.Visible = false;
        _notifyIcon?.Dispose();
    }

    private void SystemTrayApp()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Close", null, Close_OnClick);

        var assembly = Assembly.GetEntryAssembly();
        const string iconResourceName = "cswm.icon.ico";

        using (var stream = assembly!.GetManifestResourceStream(iconResourceName))
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(stream!),
                ContextMenuStrip = menu,
                Text = "cswm",
                Visible = true,
            };
        }
        _notifyIcon.Click += NotifyIcon_OnClick;

        // message loop
        Application.Run();
    }

    private void NotifyIcon_OnClick(object? sender, EventArgs e)
        => _bus.Publish(new ResetTrackedWindowsEvent());

    private void Close_OnClick(object? sender, EventArgs e)
        => _bus.Publish(new ExitApplicationEvent());
}