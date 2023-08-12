using cswm.Events;
using cswm.WinApi;
using cswm.WindowManagement.Tracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace cswm.WindowManagement.Services;

public class WindowTrackingService : IService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IWindowTrackingStrategy _strategy;
    private readonly MessageBus _bus;
    private readonly HashSet<Window> _windows = new();
    private readonly ISet<IDisposable> _eventSubscriptions = new HashSet<IDisposable>();

    public IReadOnlyCollection<Window> Windows => _windows.ToArray();

    public delegate void OnTrackedWindowsResetDelegate();
    public delegate void OnTrackedWindowChangeDelegate(Window window);

    public OnTrackedWindowsResetDelegate OnTrackedWindowsReset = null!;
    public OnTrackedWindowChangeDelegate OnWindowTrackingStart = null!;
    public OnTrackedWindowChangeDelegate OnWindowtrackingStop = null!;
    public OnTrackedWindowChangeDelegate OnWindowMoved = null!;

    /// <summary>
    /// Tracks windows without owners, like Alt + Tab.
    /// </summary>
    /// <remarks>
    /// Some windows tracked may be minimized, or not visible.
    /// </remarks>    
    /// <param name="logger"></param>
    /// <param name="strategy"></param>
    /// <param name="bus"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public WindowTrackingService(ILogger<WindowTrackingService> logger, IWindowTrackingStrategy strategy, MessageBus bus)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
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
        Subscribe(@event => @event is WindowEvent, @event => On_WindowEvent((@event as WindowEvent)!));
        Subscribe(@event => @event is ResetTrackedWindowsEvent, _ => ResetTrackedWindows());

        void Subscribe(Func<Event, bool> predicate, Action<Event> action)
        {
            var subscription = _bus.Events.Where(predicate).Subscribe(action);
            _eventSubscriptions.Add(subscription);
        }
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
        OnTrackedWindowsReset?.Invoke();
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
            OnWindowMoved.Invoke(window);
        }
    }

    private bool TryStartTracking(Window window)
    {
        var startedTracking = _windows.Add(window);
        if (startedTracking)
        {

            _logger.LogDebug("Started tracking window {window}", window);
            OnWindowTrackingStart?.Invoke(window);
        }
        return startedTracking;
    }

    private bool TryStopTracking(Window window)
    {
        var stoppedTracking = _windows.Remove(window);
        if (stoppedTracking)
        {
            _logger.LogDebug("Stopped tracking window {window}", window);
            OnWindowtrackingStop?.Invoke(window);
        }
        return stoppedTracking;
    }
}