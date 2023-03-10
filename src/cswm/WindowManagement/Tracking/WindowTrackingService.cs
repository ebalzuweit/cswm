using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using cswm.Events;
using cswm.WinApi;

namespace cswm.WindowManagement.Tracking;

public class WindowTrackingService
{
    private readonly IWindowTrackingStrategy _strategy;
    private readonly MessageBus _bus;
    private readonly HashSet<Window> _windows = new();

    public Window[] Windows => _windows.ToArray();

    public delegate void OnTrackedWindowsResetDelegate();
    public delegate void OnTrackedWindowChangeDelegate(Window window);

    public OnTrackedWindowsResetDelegate OnTrackedWindowsReset = null!;
    public OnTrackedWindowChangeDelegate OnWindowTrackingStart = null!;
    public OnTrackedWindowChangeDelegate OnWindowtrackingStop = null!;
    public OnTrackedWindowChangeDelegate OnWindowMoved = null!;

    public WindowTrackingService(IWindowTrackingStrategy strategy, MessageBus bus)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));

        _bus.Events.Where(@event => @event is WindowEvent)
            .Subscribe(@event => On_WindowEvent((@event as WindowEvent)!));
        _bus.Events.Where(@event => @event is ResetTrackedWindowsEvent)
            .Subscribe(_ => ResetTrackedWindows());
    }

    private void ResetTrackedWindows()
    {
        _windows.Clear();
        var handles = User32.EnumWindows();
        var newWindows = handles.Select(h => new Window(h))
            .Where(IsWindowVisible)
            .Where(_strategy.ShouldTrack);
        foreach (var w in newWindows)
            _windows.Add(w);
        OnTrackedWindowsReset?.Invoke();
    }

    private bool IsWindowVisible(Window window)
    {
        var isVisible = User32.IsWindowVisible(window.hWnd);
        if (isVisible == false)
            return false;

        _ = DwmApi.DwmGetWindowAttribute(window.hWnd, DwmWindowAttribute.DWMWA_CLOAKED, out var isCloaked, Marshal.SizeOf<bool>());
        if (isCloaked)
            return false;

        return true;
    }

    private readonly EventConstant[] _startTrackingEvents = new[] { EventConstant.EVENT_OBJECT_SHOW, EventConstant.EVENT_SYSTEM_MINIMIZEEND, EventConstant.EVENT_OBJECT_LOCATIONCHANGE };
    private readonly EventConstant[] _stopTrackingEvents = new[] { EventConstant.EVENT_OBJECT_HIDE, EventConstant.EVENT_SYSTEM_MINIMIZESTART };
    private void On_WindowEvent(WindowEvent @event)
    {
        var window = new Window(@event.hWnd);
        var shouldTrack = _strategy.ShouldTrack(window);
        if (shouldTrack == false)
            return;

        // ignore minimized / maximized windows
        var isIconic = User32.IsIconic(window.hWnd);
        var isZoomed = User32.IsZoomed(window.hWnd);
        if (isIconic || isZoomed)
        {
            if (_windows.Remove(window))
                OnWindowtrackingStop?.Invoke(window);
            return;
        }

        var isWindowVisible = IsWindowVisible(window);
        if (isWindowVisible && _startTrackingEvents.Contains(@event.EventType))
            if (_windows.Add(window))
                OnWindowTrackingStart?.Invoke(window);
        if (isWindowVisible == false && _stopTrackingEvents.Contains(@event.EventType))
            if (_windows.Remove(window))
                OnWindowtrackingStop?.Invoke(window);
        if (@event.EventType == EventConstant.EVENT_SYSTEM_MOVESIZEEND)
            OnWindowMoved?.Invoke(window);
    }
}