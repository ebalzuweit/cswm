using cswm.Events;
using cswm.WinApi;
using cswm.WindowManagement.Arrangement;
using cswm.WindowManagement.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cswm.WindowManagement.Services;

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
            var arrangement = movedWindow is null
                ? strategy.Arrange(layout)
                : strategy.ArrangeOnWindowMove(layout, movedWindow);
            if (arrangement is null)
                continue;
            foreach (var windowLayout in arrangement.Windows)
                SetWindowPos(windowLayout.Window, windowLayout.Position);
        }
    }

    private bool SetWindowPos(Window window, Rect position)
    {
        var windowsPadding = (window.Position.Width - window.ClientPosition.Width) / 2;
        var adjustedPosition = new Rect(
            left: position.Left - windowsPadding,
            top: position.Top,
            right: position.Right + windowsPadding,
            bottom: position.Bottom + windowsPadding);

        if (_options.DoNotManage)
            return true;
        return User32.SetWindowPos(
            window.hWnd,
            HwndInsertAfterFlags.HWND_NOTOPMOST,
            x: adjustedPosition.Left,
            y: adjustedPosition.Top,
            cx: adjustedPosition.Width,
            cy: adjustedPosition.Height,
            SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
    }
}