using cswm.Events;
using cswm.WinApi;
using System;

namespace cswm.Tracking.Events;

public class WindowMovedEvent : Event
{
    private readonly Window _window;

    public WindowMovedEvent(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        _window = window;
    }

    public Window Window => _window;
}