using cswm.Events;
using Windows.Win32.Foundation;

namespace cswm.WinApi;

public class WindowEvent : Event
{
	internal HWND hWnd { get; init; }
	public EventConstant EventType { get; init; }

	internal WindowEvent(HWND hWnd, EventConstant eventType)
	{
		this.hWnd = hWnd;
		this.EventType = eventType;
	}

	public override string ToString() => $"{EventType} hWnd: {hWnd}";
}