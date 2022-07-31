using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using cswm.Events;
using cswm.WinApi;
using Microsoft.Extensions.Logging;

namespace cswm.WindowManagement;

public class WindowTrackingService
{
    private readonly ILogger? _logger;
    private readonly MessageBus _bus;
    private readonly HashSet<Window> _windows = new HashSet<Window>();

    public Window[] Windows => _windows.ToArray();

    public delegate void OnTrackedWindowsResetDelegate();
    public delegate void OnTrackedWindowChangeDelegate(Window window);

    public OnTrackedWindowsResetDelegate OnTrackedWindowsReset = null!;
    public OnTrackedWindowChangeDelegate OnWindowTrackingStart = null!;
    public OnTrackedWindowChangeDelegate OnWindowtrackingStop = null!;

    public WindowTrackingService(ILogger<WindowTrackingService> logger, MessageBus bus)
    {
        _logger = logger;
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
            .Where(ShouldTrackWindow)
            .Where(IsWindowVisible);
        foreach (var w in newWindows)
            _windows.Add(w);
        OnTrackedWindowsReset?.Invoke();
    }

    private bool ShouldTrackWindow(Window window)
    {
        const long requiredStyles = (long)(WindowStyle.WS_THICKFRAME | WindowStyle.WS_MAXIMIZEBOX | WindowStyle.WS_MINIMIZEBOX);
        const long blockedStyles = (long)(WindowStyle.WS_CHILD);
        const long blockedExStyles = (long)(ExtendedWindowStyle.WS_EX_NOACTIVATE | ExtendedWindowStyle.WS_EX_TOOLWINDOW);

        var windowStyles = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_STYLE);
        var windowExStyles = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_EXSTYLE);

        if ((windowStyles & requiredStyles) == 0)
            return false;
        if ((windowStyles & blockedStyles) != 0)
            return false;
        if ((windowExStyles & blockedExStyles) != 0)
            return false;

        var isAltTabWindow = User32.IsAltTabWindow(window.hWnd);
        if (isAltTabWindow == false)
            return false;

        return true;
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

    private readonly EventConstant[] _startTrackingEvents = new[] { EventConstant.EVENT_OBJECT_SHOW, EventConstant.EVENT_SYSTEM_MINIMIZEEND };
    private readonly EventConstant[] _stopTrackingEvents = new[] { EventConstant.EVENT_OBJECT_HIDE, EventConstant.EVENT_SYSTEM_MINIMIZESTART };
    private void On_WindowEvent(WindowEvent @event)
    {
        var window = new Window(@event.hWnd);
        var shouldNotTrackWindow = ShouldTrackWindow(window) == false;
        if (shouldNotTrackWindow)
            return;

        bool windowVisible = IsWindowVisible(window);
        if (windowVisible && _startTrackingEvents.Contains(@event.EventType))
            if (_windows.Add(window))
                OnWindowTrackingStart?.Invoke(window);
        if (windowVisible == false && _stopTrackingEvents.Contains(@event.EventType))
            if (_windows.Remove(window))
                OnWindowtrackingStop?.Invoke(window);
    }
}