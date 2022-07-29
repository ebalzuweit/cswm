namespace cswm.WinApi;

/// <summary>
/// <see href="https://docs.microsoft.com/en-us/windows/win32/winauto/event-constants"/>
/// </summary>
public enum EventConstant : uint
{
    EVENT_SYSTEM_FOREGROUND = 0x0003,
    EVENT_SYSTEM_MOVESIZEEND = 0x000b,
    EVENT_SYSTEM_MINIMIZESTART = 0x0016,
    EVENT_SYSTEM_MINIMIZEEND = 0x0017,
    EVENT_OBJECT_DESTROY = 0x8001,
    EVENT_OBJECT_SHOW = 0x8002,
    EVENT_OBJECT_HIDE = 0x8003,
    EVENT_OBJECT_LOCATIONCHANGE = 0x800b,
}