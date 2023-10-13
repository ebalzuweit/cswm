using cswm.Arrangement;
using cswm.Arrangement.Events;
using cswm.Events;
using cswm.Services.Arrangement;
using cswm.Services.Tracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace cswm.Services.SystemTray;

/// <summary>
/// Context menu for <see cref="SystemTrayService"/>.
/// </summary>
public class SystemTrayMenu
{
    private readonly IServiceProvider _provider;
    private readonly MessageBus _bus;
    private readonly WindowTrackingService _trackingService;
    private readonly WindowArrangementService _layoutService;

    public SystemTrayMenu(
        IServiceProvider provider,
        MessageBus bus,
        WindowTrackingService trackingService,
        WindowArrangementService layoutService)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(layoutService);

        _provider = provider;
        _bus = bus;
        _trackingService = trackingService;
        _layoutService = layoutService;
    }

    public ToolStripItem[] BuildTrayMenu()
    {
        var items = new List<ToolStripItem>
        {
            BuildAboutMenuItem(),
            new ToolStripSeparator()
        };
        var monitorLayouts = _trackingService.GetCurrentLayouts();
        items.AddRange(BuildMonitorMenuItems(monitorLayouts));
        items.Add(new ToolStripSeparator());
        items.Add(BuildCloseMenuItem());

        return items.ToArray();
    }

    private ToolStripMenuItem BuildAboutMenuItem()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var version = assembly.GetName().Version!;
        var caption = $"cswm v{version.Major}.{version.Minor}.{version.Build}";
        return new(caption, null, OnClick);

        void OnClick(object? sender, EventArgs eventArgs)
            => Process.Start(new ProcessStartInfo("https://github.com/ebalzuweit/cswm") { UseShellExecute = true });
    }

    private ToolStripMenuItem BuildCloseMenuItem()
    {
        return new("Close", null, OnClick);

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new ExitApplicationEvent());
    }

    private ToolStripMenuItem[] BuildMonitorMenuItems(IEnumerable<MonitorLayout> layouts)
    {
        return layouts.Select(BuildMonitorMenuItem).ToArray();

        ToolStripMenuItem BuildMonitorMenuItem(MonitorLayout layout)
        {
            var items = BuildMonitorMenuList(layout);
            return new ToolStripMenuItem(text: layout.Monitor.DeviceName, image: null, dropDownItems: items);
        }

        ToolStripMenuItem[] BuildMonitorMenuList(MonitorLayout layout)
        {
            var items = new List<ToolStripMenuItem>()
            {
                BuildArrangementMenuItem<SplitArrangementStrategy>(layout),
                BuildArrangementMenuItem<SilentArrangementStrategy>(layout)
            };
            return items.ToArray();
        }
    }

    private ToolStripMenuItem BuildArrangementMenuItem<T>(MonitorLayout layout)
        where T : IArrangementStrategy
    {
        var n = typeof(T).Name;
        var name = n[..n.IndexOf("ArrangementStrategy")];
        var isChecked = typeof(T) == _layoutService.GetArrangement(layout.Monitor.hMonitor).GetType();
        return new(name, null, OnClick)
        {
            Checked = isChecked
        };

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new SetArrangementStrategyEvent((IArrangementStrategy)_provider.GetService(typeof(T))!, layout.Monitor));
    }
}