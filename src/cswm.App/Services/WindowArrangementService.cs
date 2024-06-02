using cswm.Arrangement;
using cswm.Arrangement.Events;
using cswm.Events;
using cswm.Options;
using cswm.Tracking.Events;
using cswm.WinApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace cswm.App.Services;

/// <summary>
/// Performs window arrangement, using an <see cref="IArrangementStrategy"/> for each monitor.
/// </summary>
public sealed class WindowArrangementService : IService
{
    private readonly ILogger _logger;
    private readonly WindowManagementOptions _options;
    private readonly MessageBus _bus;
    private readonly WindowTrackingService _trackingService;
    private readonly IArrangementStrategy _defaultArrangementStrategy;

    // TODO: store initial window positions in Start, restore positions on Stop
    private List<IDisposable> _subscriptions = new();
    private Dictionary<IntPtr, IArrangementStrategy> _monitorStrategies = new();
    private Dictionary<IntPtr, MonitorLayout> _prevArrangements = new();

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

    /// <inheritdoc/>
    public void Start()
    {
        Subscribe<SetArrangementStrategyEvent>(OnSetArrangementStrategy);
        Subscribe<StartTrackingWindowEvent>(e => StartArrangingWindow(e.Window));
        Subscribe<StopTrackingWindowEvent>(e => StopArrangingWindow(e.Window));
        Subscribe<WindowMovedEvent>(OnWindowMoved);
        Subscribe<OnTrackedWindowsResetEvent>(OnWindowTrackingReset);
        Subscribe<SetWindowFlaggedEvent>(OnWindowFlagged);

        // Setup initial monitor strategies
        foreach (var monitorLayout in _trackingService.GetCurrentLayouts())
        {
            _monitorStrategies[monitorLayout.Monitor.hMonitor] = _defaultArrangementStrategy;
            UpdateArrangement(monitorLayout.Monitor.hMonitor);
        }

        void Subscribe<T>(Action<T> action)
            where T : Event
            => _subscriptions.Add(_bus.Subscribe<T>(action));
    }

    /// <inheritdoc/>
    public void Stop()
    {
        foreach (var subscription in _subscriptions)
            subscription.Dispose();
        _subscriptions.Clear();
    }

    /// <summary>
    /// Get the arrangement strategy for a monitor.
    /// </summary>
    /// <param name="hMon">Handle for the monitor.</param>
    /// <returns></returns>
    public IArrangementStrategy GetStrategy(IntPtr hMon)
    {
        if (_monitorStrategies.ContainsKey(hMon) == false)
        {
            _logger.LogWarning("Failed to lookup arrangement strategy for monitor [{hMonitor}]", hMon);
            _monitorStrategies[hMon] = _defaultArrangementStrategy;
            return _defaultArrangementStrategy;
        }
        return _monitorStrategies[hMon];
    }

    /// <summary>
    /// Apply a given arrangement for a monitor.
    /// </summary>
    /// <param name="arrangement">Arrangement for a monitor.</param>
    private void ApplyArrangement(MonitorLayout? arrangement)
    {
        if (arrangement == default)
        {
            return;
        }

        _prevArrangements[arrangement.Monitor.hMonitor] = arrangement;

        if (_options.DoNotManage)
        {
            return;
        }

        // Move windows into position
        foreach (var windowLayout in arrangement.Windows)
        {
            // TODO: skip if already in position
            User32.SetWindowPos(
                windowLayout.Window.hWnd,
                HwndInsertAfterFlags.HWND_NOTOPMOST,
                x: windowLayout.Position.Left,
                y: windowLayout.Position.Top,
                cx: windowLayout.Position.Width,
                cy: windowLayout.Position.Height,
                SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
        }
    }

    /// <summary>
    /// Helper function to fetch strategy and previous (or current) arrangement, for a monitor.
    /// </summary>
    /// <param name="hMon">Handle for the monitor.</param>
    /// <returns></returns>
    private (IArrangementStrategy strategy, MonitorLayout? arrangement) GetStrategyAndPrevOrCurrArrangement(IntPtr hMon)
    {
        var strategy = GetStrategy(hMon);
        var arrangement = GetPreviousOrCurrentArrangement(hMon);

        return (strategy, arrangement);
    }

    /// <summary>
    /// Get the previous or current arrangement for a monitor.
    /// </summary>
    /// <param name="hMon">Handle for the monitor.</param>
    /// <returns></returns>
    private MonitorLayout? GetPreviousOrCurrentArrangement(IntPtr hMon)
    {
        if (_prevArrangements.ContainsKey(hMon))
        {
            return _prevArrangements[hMon];
        }
        return _trackingService
            .GetCurrentLayouts() // TODO: only fetch layout for monitor in consideration
            .Where(x => x.Monitor.hMonitor == hMon)
            .FirstOrDefault();
    }

    /// <summary>
    /// Set the arrangement strategy for all monitors.
    /// </summary>
    /// <remarks>
    /// This will trigger arrangement for all monitors.
    /// </remarks>
    /// <param name="strategy">Strategy to use for arranging the windows.</param>
    private void SetStrategy(IArrangementStrategy strategy)
    {
        foreach (var hMon in _monitorStrategies.Keys)
        {
            SetStrategyForMonitor(hMon, strategy);
        }
    }

    /// <summary>
    /// Set the arrangement strategy for a <see cref="Monitor"/>.
    /// </summary>
    /// <remarks>
    /// This will trigger arrangement for the <see cref="Monitor"/>.
    /// </remarks>
    /// <param name="hMon">Handle for the monitor.</param>
    /// <param name="strategy">Strategy to use for arranging the windows.</param>
    private void SetStrategyForMonitor(IntPtr hMon, IArrangementStrategy strategy)
    {
        _monitorStrategies[hMon] = strategy;
        UpdateArrangement(hMon);
    }

    /// <summary>
    /// Update the arrangement for a <see cref="Monitor"/>.
    /// </summary>
    /// <param name="hMon">Handle for the monitor.</param>
    private void UpdateArrangement(IntPtr hMon)
    {
        var (strategy, current) = GetStrategyAndPrevOrCurrArrangement(hMon);
        if (current == default)
        {
            _logger.LogError("Failed to get previous or current arrangement for monitor [{hMonitor}]", hMon);
            return;
        }
        var updateArrangement = strategy.Arrange(current);
        ApplyArrangement(updateArrangement);
    }

    #region Event Handlers

    private void OnSetArrangementStrategy(SetArrangementStrategyEvent @event)
    {
        if (@event.AllMonitors)
        {
            SetStrategy(@event.Strategy);
        }
        else
        {
            SetStrategyForMonitor(@event.Monitor!.hMonitor, @event.Strategy);
        }
    }

    private void OnWindowTrackingReset(OnTrackedWindowsResetEvent @event)
    {
        _prevArrangements.Clear();
        foreach (var (hMon, _) in _monitorStrategies)
        {
            UpdateArrangement(hMon);
        }
    }

    private void StartArrangingWindow(Window window)
    {
        var hMon = User32.MonitorFromWindow(window.hWnd, MonitorFlags.DefaultToNearest);
        var (strategy, currArrangement) = GetStrategyAndPrevOrCurrArrangement(hMon);
        if (currArrangement == default)
        {
            _logger.LogWarning("Start Tracking: Failed to get previous and current arrangement for monitor [{hMonitor}]", hMon);
            return;
        }
        var updatedArrangement = strategy.AddWindow(currArrangement, window);
        ApplyArrangement(updatedArrangement);
    }

    private void StopArrangingWindow(Window window)
    {
        var hMon = User32.MonitorFromWindow(window.hWnd, MonitorFlags.DefaultToNearest);
        var (strategy, currArrangement) = GetStrategyAndPrevOrCurrArrangement(hMon);
        if (currArrangement == default)
        {
            _logger.LogWarning("Stop Tracking: Failed to get previous and current arrangement for monitor [{hMonitor}]", hMon);
            return;
        }
        var updatedArrangement = strategy.RemoveWindow(currArrangement, window);
        ApplyArrangement(updatedArrangement);
    }

    private void OnWindowMoved(WindowMovedEvent @event)
    {
        var hMon = User32.MonitorFromWindow(@event.Window.hWnd, MonitorFlags.DefaultToNearest);
        var (strategy, currArrangement) = GetStrategyAndPrevOrCurrArrangement(hMon);
        if (currArrangement == default)
        {
            _logger.LogWarning("Window Move: Failed to get previous and current arrangement for monitor [{hMonitor}]", hMon);
            return;
        }
        var cursorPosition = new Point();
        if (User32.GetCursorPos(ref cursorPosition) == false)
        {
            _logger.LogWarning("Window Move: Failed to get cursor position!");
            return;
        }
        var updatedArrangement = strategy.MoveWindow(currArrangement, @event.Window, cursorPosition);
        ApplyArrangement(updatedArrangement);
    }

    private void OnWindowFlagged(SetWindowFlaggedEvent @event)
    {
        if (_options.DoNotManage)
        {
            return;
        }

        var flags = @event.Flagged
            ? HwndInsertAfterFlags.HWND_TOPMOST
            : HwndInsertAfterFlags.HWND_NOTOPMOST;

        User32.SetWindowPos(
            @event.Window.hWnd,
            flags,
            x: @event.Window.Position.Left,
            y: @event.Window.Position.Top,
            cx: @event.Window.Position.Width,
            cy: @event.Window.Position.Height,
            SetWindowPosFlags.SWP_ASYNCWINDOWPOS | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);

        if (@event.Flagged)
        {
            StopArrangingWindow(@event.Window);
        }
        else
        {
            StartArrangingWindow(@event.Window);
        }
    }

    #endregion Event Handlers
}