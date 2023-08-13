using cswm.Events;
using System;

namespace cswm.WindowManagement.Events;

public class StartTrackingWindowEvent : Event
{
    private readonly Window _window;

    public StartTrackingWindowEvent(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        _window = window;
    }

    public Window Window => _window;
}