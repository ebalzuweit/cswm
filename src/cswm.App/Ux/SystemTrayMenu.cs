using cswm.App.Events;
using cswm.App.Services;
using cswm.Arrangement;
using cswm.Arrangement.Events;
using cswm.Events;
using cswm.Tracking.Events;
using cswm.WinApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace cswm.App.Ux;

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
        var monitorLayouts = _trackingService.GetCurrentLayouts(includeFlaggedWindows: true);
        items.AddRange(BuildMonitorMenuItems(monitorLayouts));
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

    private ToolStripMenuItem[] BuildMonitorMenuItems(IEnumerable<MonitorLayout> monitorLayouts)
    {
        return monitorLayouts.Count() == 1
            ? BuildMenuListForMonitor(monitorLayouts.Single())
            : monitorLayouts.Select(BuildMenuListItemForMonitor).ToArray();

        ToolStripMenuItem BuildMenuListItemForMonitor(MonitorLayout monitorLayout)
            => new ToolStripMenuItem(text: monitorLayout.Monitor.DeviceName ?? "[MONITOR]", image: null, dropDownItems: BuildMenuListForMonitor(monitorLayout));

        ToolStripMenuItem[] BuildMenuListForMonitor(MonitorLayout monitorLayout)
        {
            var arrangeList = new ToolStripMenuItem(text: "Arrangement", image: null, dropDownItems: BuildArrangementListForMonitor(monitorLayout.Monitor));
            var windowList = new ToolStripMenuItem(text: "Windows", image: null, dropDownItems: BuildWindowListForMonitor(monitorLayout));
            return [arrangeList, windowList];
        }

        ToolStripItem[] BuildArrangementListForMonitor(Monitor monitor)
            => [
                BuildResetArrangementStrategyMenuItem(monitor),
                new ToolStripSeparator(),
                BuildArrangementMenuItem<SplitArrangementStrategy>(monitor),
                BuildArrangementMenuItem<SilentArrangementStrategy>(monitor)
            ];

        ToolStripItem[] BuildWindowListForMonitor(MonitorLayout monitorLayout)
        {
            var windowItems = monitorLayout.Windows.Select(x => BuildWindowMenuItem(x.Window));
            var windowList = new List<ToolStripItem>()
            {
                BuildResetTrackedWindowsMenuItem(),
                new ToolStripSeparator()
            };
            windowList.AddRange(windowItems);

            return windowList.ToArray();
        }
    }

    private ToolStripMenuItem BuildResetTrackedWindowsMenuItem()
    {
        return new("Reset Tracked Windows", null, OnClick);

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new ResetTrackedWindowsEvent());
    }

    private ToolStripMenuItem BuildResetArrangementStrategyMenuItem(Monitor monitor)
    {
        return new("Reset Arrangement", null, OnClick);

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new ResetArrangementStrategyEvent());
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

    private ToolStripMenuItem BuildWindowMenuItem(Window window)
    {
        var name = window.Caption ?? window.ClassName ?? "[WINDOW]";
        if (name.Length > 20)
            name = name[..17] + "...";
        var isFlagged = _trackingService.IsFlaggedWindow(window);
        return new(name, null, OnClick)
        {
            Checked = isFlagged
        };

        void OnClick(object? sender, EventArgs eventArgs)
            => _bus.Publish(new SetWindowFlaggedEvent(window, !isFlagged));
    }
}