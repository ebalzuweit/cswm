using cswm.Events;
using Windows.Win32.Foundation;

namespace cswm.WinApi.Events;

public class WindowEvent : Event
{
    public HWND hWnd { get; init; }
    public EventConstant EventType { get; init; }

    public WindowEvent(HWND hWnd, EventConstant eventType)
    {
        this.hWnd = hWnd;
        this.EventType = eventType;
    }

    public override string ToString() => $"{EventType} hWnd: {hWnd}";
}