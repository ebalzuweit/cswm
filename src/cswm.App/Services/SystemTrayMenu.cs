using cswm.Arrangement;
using cswm.Arrangement.Events;
using cswm.Events;
using cswm.WinApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace cswm.App.Services;

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
        var monitors = _trackingService.GetCurrentLayouts().Select(x => x.Monitor);
        items.AddRange(BuildMonitorMenuItems(monitors));
        items.Add(new ToolStripSeparator());
        items.Add(BuildCloseMenuItem());

        return items.ToArray();
    }

    private ToolStripMenuItem BuildAboutMenuItem()
    {
        const string AppName = "cswm";
        const string AboutUrl = "https://github.com/ebalzuweit/cswm";

        var assembly = Assembly.GetEntryAssembly()!;
        var version = assembly.GetName().Version!;
        var caption = $"{AppName} v{version.Major}.{version.Minor}.{version.Build}";
        return new(caption, null, OnClick);

        void OnClick(object? sender, EventArgs eventArgs)
            => Process.Start(new ProcessStartInfo(AboutUrl) { UseShellExecute = true });
    }

    private ToolStripMenuItem BuildCloseMenuItem()
    {
        return new("Close", null, OnClick);

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new ExitApplicationEvent());
    }

    private ToolStripMenuItem[] BuildMonitorMenuItems(IEnumerable<Monitor> monitors)
    {
        return monitors.Select(BuildMonitorMenuItem).ToArray();

        ToolStripMenuItem BuildMonitorMenuItem(Monitor monitor)
        {
            var items = BuildMonitorMenuList(monitor);
            return new ToolStripMenuItem(text: monitor.DeviceName, image: null, dropDownItems: items);
        }

        ToolStripMenuItem[] BuildMonitorMenuList(Monitor monitor)
        {
            var items = new List<ToolStripMenuItem>()
            {
                BuildArrangementMenuItem<SplitArrangementStrategy>(monitor),
                BuildArrangementMenuItem<SilentArrangementStrategy>(monitor)
            };
            return items.ToArray();
        }
    }

    private ToolStripMenuItem BuildArrangementMenuItem<T>(Monitor monitor)
        where T : IArrangementStrategy
    {
        var n = typeof(T).Name;
        var name = n[..n.IndexOf("ArrangementStrategy")];
        var isChecked = typeof(T) == _layoutService.GetStrategy(monitor.hMonitor).GetType();
        return new(name, null, OnClick)
        {
            Checked = isChecked
        };

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new SetArrangementStrategyEvent((IArrangementStrategy)_provider.GetService(typeof(T))!, monitor));
    }
}