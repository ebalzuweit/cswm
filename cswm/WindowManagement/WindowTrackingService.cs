using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

    public delegate void OnTrackedWindowChange(Window window);

    public OnTrackedWindowChange OnWindowTrackingStart = null!;
    public OnTrackedWindowChange OnWindowtrackingStop = null!;

    public WindowTrackingService(ILogger<WindowTrackingService> logger, MessageBus bus)
    {
        _logger = logger;
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));

        _bus.Events.Where(@event => @event is WindowEvent)
            .Subscribe(@event => On_WindowEvent((@event as WindowEvent)!));
    }

    public void ResetTrackedWindows()
    {
        _windows.Clear();
        var handles = User32.EnumWindows();
        var newWindows = handles.Select(h => new Window(h))
            .Where(ShouldTrackWindow);
        foreach (var w in newWindows)
            _windows.Add(w);
    }

    public bool ShouldTrackWindow(Window window)
    {
        return true;
    }

    private void On_WindowEvent(WindowEvent @event)
    {
        var window = new Window(@event.hWnd);
        var shouldNotTrackWindow = !ShouldTrackWindow(window);
        if (shouldNotTrackWindow)
            return;

        switch (@event.EventType)
        {
            case EventConstant.EVENT_OBJECT_SHOW:
                _windows.Add(window);
                OnWindowTrackingStart?.Invoke(window);
                break;
            case EventConstant.EVENT_OBJECT_HIDE:
                _windows.Remove(window);
                OnWindowtrackingStop?.Invoke(window);
                break;
        }
    }
}