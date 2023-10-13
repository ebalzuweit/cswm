using cswm.Arrangement;
using cswm.Events;
using cswm.Tracking;
using cswm.Tracking.Events;
using cswm.WinApi;
using cswm.WinApi.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace cswm.Services.Tracking;

/// <summary>
/// Tracks active windows, like Alt + Tab.
/// </summary>
public class WindowTrackingService : IService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IWindowTrackingStrategy _strategy;
    private readonly MessageBus _bus;
    private readonly HashSet<Window> _windows = new();
    private readonly ISet<IDisposable> _eventSubscriptions = new HashSet<IDisposable>();

    public IReadOnlyCollection<Window> Windows => _windows.ToArray();

    public WindowTrackingService(ILogger<WindowTrackingService> logger, IWindowTrackingStrategy strategy, MessageBus bus)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(bus);

        _logger = logger;
        _strategy = strategy;
        _bus = bus;
    }

    public void Start()
    {
        ResetTrackedWindows();
        SubscribeToEvents();
    }

    public void Stop()
    {
        foreach (var subscription in _eventSubscriptions)
            subscription.Dispose();
        _eventSubscriptions.Clear();
    }

    public void Dispose()
    {
        Stop();

        GC.SuppressFinalize(this);
    }

    public IEnumerable<MonitorLayout> GetCurrentLayouts()
        => User32.EnumDisplayMonitors()
            .Select(hMonitor =>
            new MonitorLayout(
                new Monitor(hMonitor),
                Windows
                    .Where(w => User32.MonitorFromWindow(w.hWnd, MonitorFlags.DefaultToNearest) == hMonitor)
                    .Select(w => new WindowLayout(w, w.Position))
            ));

    public bool IsWindowVisible(Window window)
    {
        var isVisible = User32.IsWindowVisible(window.hWnd);
        if (isVisible == false)
            return false;

        _ = DwmApi.DwmGetWindowAttribute(window.hWnd, DwmWindowAttribute.DWMWA_CLOAKED, out var isCloaked, Marshal.SizeOf<bool>());
        if (isCloaked)
            return false;

        return true;
    }

    public bool IsNotMinOrMaximized(Window window) => !User32.IsIconic(window.hWnd) && !User32.IsZoomed(window.hWnd);

    private void SubscribeToEvents()
    {
        _eventSubscriptions.Add(_bus.Subscribe<WindowEvent>(On_WindowEvent));
        _eventSubscriptions.Add(_bus.Subscribe<ResetTrackedWindowsEvent>(_ => ResetTrackedWindows()));
    }

    private void ResetTrackedWindows()
    {
        _windows.Clear();
        var handles = User32.EnumWindows();
        var newWindows = handles.Select(h => new Window(h))
            .Where(IsNotMinOrMaximized)
            .Where(_strategy.ShouldTrack);
        foreach (var w in newWindows)
        {
            _windows.Add(w);
        }
        _logger.LogDebug("Reset tracked windows, {WindowsCount} windows tracked.", _windows.Count);
        _bus.Publish(new OnTrackedWindowsResetEvent());
    }

    private readonly EventConstant[] _startTrackingEvents = new[] { EventConstant.EVENT_OBJECT_SHOW, EventConstant.EVENT_SYSTEM_MINIMIZEEND, EventConstant.EVENT_OBJECT_LOCATIONCHANGE };
    private readonly EventConstant[] _stopTrackingEvents = new[] { EventConstant.EVENT_OBJECT_HIDE, EventConstant.EVENT_SYSTEM_MINIMIZESTART };
    private void On_WindowEvent(WindowEvent @event)
    {
        var window = new Window(@event.hWnd);
        var tracking = _windows.Any(x => x.hWnd == @event.hWnd);
        if (tracking && _stopTrackingEvents.Contains(@event.EventType))
        {
            TryStopTracking(window);
        }

        var shouldTrack = _strategy.ShouldTrack(window);
        if (shouldTrack == false)
            return;
        if (_startTrackingEvents.Contains(@event.EventType))
            TryStartTracking(window);
        else if (@event.EventType == EventConstant.EVENT_SYSTEM_MOVESIZEEND)
        {
            _logger.LogDebug("Window moved {window}", window);
            _bus.Publish(new WindowMovedEvent(window));
        }
    }

    private bool TryStartTracking(Window window)
    {
        var startedTracking = _windows.Add(window);
        if (startedTracking)
        {

            _logger.LogDebug("Started tracking window {window}", window);
            _bus.Publish(new StartTrackingWindowEvent(window));
        }
        return startedTracking;
    }

    private bool TryStopTracking(Window window)
    {
        var stoppedTracking = _windows.Remove(window);
        if (stoppedTracking)
        {
            _logger.LogDebug("Stopped tracking window {window}", window);
            _bus.Publish(new StopTrackingWindowEvent(window));
        }
        return stoppedTracking;
    }
}