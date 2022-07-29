using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace cswm;

public class SystemTrayService
{
    private NotifyIcon? _notifyIcon;

    public SystemTrayService()
    {

    }

    public void Start()
    {
        var thread = new Thread(() => SystemTrayApp())
        {
            Name = "cswmtray"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
#if DEBUG
        Console.WriteLine($"Started thread '{thread.Name}'.");
#endif
    }

    public void Stop()
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

        using (var stream = assembly.GetManifestResourceStream(iconResourceName))
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(stream),
                ContextMenuStrip = menu,
                Text = "cswm",
                Visible = true,
            };
        }

        // message loop
        Application.Run();
    }

    private void Close_OnClick(object? sender, EventArgs e)
    {

    }
}