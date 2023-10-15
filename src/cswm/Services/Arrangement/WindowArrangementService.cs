using cswm.Arrangement;
using cswm.Arrangement.Events;
using cswm.Events;
using cswm.Services.Tracking;
using cswm.Tracking.Events;
using cswm.WinApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace cswm.Services.Arrangement;

/// <summary>
/// Performs window layout, based on an <see cref="IArrangementStrategy"/>.
/// </summary>
public sealed class WindowArrangementService : IService
{
    private readonly ILogger _logger;
    private readonly WindowManagementOptions _options;
    private readonly MessageBus _bus;
    private readonly WindowTrackingService _trackingService;
    private readonly IArrangementStrategy _defaultArrangementStrategy;

    private List<IDisposable> _subscriptions = new();
    private Dictionary<IntPtr, IArrangementStrategy> _monitorStrategies = new();

    public WindowArrangementService(
        ILogger<WindowArrangementService> logger,
        IOptions<WindowManagementOptions> options,
        MessageBus bus,
        WindowTrackingService trackingService,
        SplitArrangementStrategy defaultArrangementStrategy
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(trackingService);
        ArgumentNullException.ThrowIfNull(defaultArrangementStrategy);

        _logger = logger;
        _options = options.Value;
        _bus = bus;
        _trackingService = trackingService;
        _defaultArrangementStrategy = defaultArrangementStrategy;
    }

    public IArrangementStrategy GetArrangement(IntPtr hMon)
    {
        if (_monitorStrategies.ContainsKey(hMon) == false)
            return _defaultArrangementStrategy;
        return _monitorStrategies[hMon];
    }

    public void SetArrangement(IntPtr hMon, IArrangementStrategy strategy) => _monitorStrategies[hMon] = strategy;
    public void SetAllArrangements(IArrangementStrategy strategy)
    {
        foreach (var hMon in _monitorStrategies.Keys)
        {
            SetArrangement(hMon, strategy);
        }
    }

    public void Start()
    {
        Subscribe<SetArrangementStrategyEvent>(
            @event =>
            {
                _logger.LogDebug("Applying arrangment {ArrangementType} to {Target}",
                    @event.Strategy.GetType().Name,
                    @event.AllMonitors ? "ALL" : @event.Monitor!.DeviceName);
                if (@event.AllMonitors)
                {
                    SetAllArrangements(@event.Strategy);
                    Rearrange();
                }
                else
                {
                    SetArrangement(@event.Monitor!.hMonitor, @event.Strategy);
                    Rearrange(@event.Monitor!.hMonitor);
                }
            });
        Subscribe<StartTrackingWindowEvent>(@event => OnWindowTrackingStart(@event.Window));
        Subscribe<StopTrackingWindowEvent>(@event => OnWindowTrackingStop(@event.Window));
        Subscribe<WindowMovedEvent>(@event => OnWindowMoved(@event.Window));
        Subscribe<OnTrackedWindowsResetEvent>(@event => OnWindowTrackingReset());

        var hMons = User32.EnumDisplayMonitors();
        foreach (var hMon in hMons)
        {
            _monitorStrategies[hMon] = _defaultArrangementStrategy;
        }
        Rearrange();

        void Subscribe<T>(Action<T> action)
            where T : Event
            => _subscriptions.Add(_bus.Subscribe<T>(action));
    }

    public void Stop()
    {
        foreach (var subscription in _subscriptions)
            subscription.Dispose();
        _subscriptions.Clear();
    }

    private void Rearrange(IntPtr? hMon = null)
    {
        UpdateWindowPositions(hMon: hMon);
    }

    private void OnWindowTrackingReset()
    {
        Rearrange();
    }

    private void OnWindowTrackingStart(Window window)
    {
        UpdateWindowPositions();
    }

    private void OnWindowTrackingStop(Window window)
    {
        UpdateWindowPositions();
    }

    private void OnWindowMoved(Window window)
    {
        UpdateWindowPositions(movedWindow: window);
    }

    private void UpdateWindowPositions(Window? movedWindow = default, IntPtr? hMon = null)
    {
        var monitorLayouts = _trackingService.GetCurrentLayouts()
            .Where(x => hMon is null || x.Monitor.hMonitor == hMon);

        foreach (var layout in monitorLayouts)
        {
            var strategy = _monitorStrategies[layout.Monitor.hMonitor];
            var arrangement = GetArrangement(layout, strategy);
            if (arrangement is null)
                continue;
            foreach (var windowLayout in arrangement.Windows)
                SetWindowPos(windowLayout.Window, windowLayout.Position);
        }

        MonitorLayout? GetArrangement(MonitorLayout layout, IArrangementStrategy strategy)
        {
            if (movedWindow is null)
            {
                return strategy.Arrange(layout);
            }
            else
            {
                var cursorPosition = new Point();
                if (User32.GetCursorPos(ref cursorPosition))
                {
                    return strategy.ArrangeOnWindowMove(layout, movedWindow, cursorPosition);
                }
            }
            return null;
        }
    }

    private bool SetWindowPos(Window window, Rect position)
    {
        if (_options.DoNotManage)
            return true;

        return User32.SetWindowPos(
            window.hWnd,
            HwndInsertAfterFlags.HWND_NOTOPMOST,
            x: position.Left,
            y: position.Top,
            cx: position.Width,
            cy: position.Height,
            SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
    }
}