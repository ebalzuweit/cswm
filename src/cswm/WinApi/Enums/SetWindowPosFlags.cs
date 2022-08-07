using System;

namespace cswm.WinApi;

/// <summary>
/// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos"/>
/// </summary>
[Flags]
public enum SetWindowPosFlags : uint
{
    /// <summary>
    /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
    /// </summary>
    SWP_NOACTIVATE = 0x0010,
    /// <summary>
    /// Displays the window.
    /// </summary>
    SWP_SHOWWINDOW = 0x0040,
    /// <summary>
    /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
    /// </summary>
    SWP_ASYNCWINDOWPOS = 0x4000,
}