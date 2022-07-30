using System;
using cswm.Events;

namespace cswm.WinApi;

public class WindowEvent : Event
{
    public IntPtr hWnd { get; init; }
    public EventConstant EventType { get; init; }

    public WindowEvent(IntPtr hWnd, EventConstant eventType)
    {
        this.hWnd = hWnd;
        this.EventType = eventType;
    }

    public override string ToString() => $"{EventType} hWnd: {hWnd}";
}