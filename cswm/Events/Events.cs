using System;

namespace cswm.Events;

public class Event { }

public class WindowEvent : Event
{
    public IntPtr hWnd { get; init; }

    public WindowEvent(IntPtr hWnd)
    {
        this.hWnd = hWnd;
    }
}

public class ExitApplicationEvent : Event { }

public class ForegroundWindowChangeEvent : WindowEvent
{
    public ForegroundWindowChangeEvent(IntPtr hWnd) : base(hWnd) { }
}