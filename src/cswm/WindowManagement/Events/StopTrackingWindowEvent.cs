using cswm.Events;
using System;

namespace cswm.WindowManagement.Events;

public class StopTrackingWindowEvent : Event
{
    private readonly Window _window;

    public StopTrackingWindowEvent(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        _window = window;
    }

    public Window Window => _window;
}