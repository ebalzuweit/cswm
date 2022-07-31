using System;

namespace cswm.WinApi;

/// <summary>
/// <see href="https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles"/>
/// </summary>
[Flags]
public enum ExtendedWindowStyle : long
{
    /// <summary>
    /// Forces a top-level window onto the taskbar when the window is visible.
    /// </summary>
    WS_EX_APPWINDOW = 0x00040000L,
    /// <summary>
    /// The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
    /// </summary>
    WS_EX_TOOLWINDOW = 0x00000080L,
    /// <summary>
    /// <para>A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.</para>
    /// <para>The window should not be activated through programmatic access or via keyboard navigation by accessible technology, such as Narrator.</para>
    /// <para>To activate the window, use the SetActiveWindow or SetForegroundWindow function.</para>
    /// <para>The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.</para>
    /// </summary>
    WS_EX_NOACTIVATE = 0x08000000L,
}