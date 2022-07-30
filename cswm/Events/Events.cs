using System;

namespace cswm.Events;

public class Event { }

public abstract class WindowEvent : Event
{
    public IntPtr hWnd { get; init; }

    public WindowEvent(IntPtr hWnd)
    {
        this.hWnd = hWnd;
    }
}

public class ExitApplicationEvent : Event { }

public class ForegroundChangeWindowEvent : WindowEvent
{
    public ForegroundChangeWindowEvent(IntPtr hWnd) : base(hWnd) { }
}

public class MinimizeStartWindowEvent : WindowEvent
{
    public MinimizeStartWindowEvent(IntPtr hWnd) : base(hWnd) { }
}

public class MinimizeEndWindowEvent : WindowEvent
{
    public MinimizeEndWindowEvent(IntPtr hWnd) : base(hWnd) { }
}

public class ShowWindowEvent : WindowEvent
{
    public ShowWindowEvent(IntPtr hWnd) : base(hWnd) { }
}

public class HideWindowEvent : WindowEvent
{
    public HideWindowEvent(IntPtr hWnd) : base(hWnd) { }
}