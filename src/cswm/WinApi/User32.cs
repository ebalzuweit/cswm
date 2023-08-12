using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace cswm.WinApi;

public static class User32
{
    public delegate void WinEventProc(IntPtr hWinEventHook, EventConstant eventType, IntPtr hwnd, ObjectIdentifier idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    /// <summary>
    /// Sets an event hook function for a range of events.
    /// </summary>
    /// <remarks>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook"/>
    /// <para/>
    /// <see href="https://www.pinvoke.net/default.aspx/user32.setwineventhook"/>
    /// </remarks>
    /// <param name="eventMin">Specifies the event constant for the lowest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MIN to indicate the lowest possible event value.</param>
    /// <param name="eventMax">Specifies the event constant for the highest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MAX to indicate the highest possible event value.</param>
    /// <param name="hmodWinEventProc">Handle to the DLL that contains the hook function at lpfnWinEventProc, if the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter. If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this parameter is NULL.</param>
    /// <param name="lpfnWinEventProc">Pointer to the event hook function. For more information about this function, see WinEventProc.</param>
    /// <param name="idProcess">Specifies the ID of the process from which the hook function receives events. Specify zero (0) to receive events from all processes on the current desktop.</param>
    /// <param name="idThread">Specifies the ID of the thread from which the hook function receives events. If this parameter is zero, the hook function is associated with all existing threads on the current desktop.</param>
    /// <param name="dwFlags">Flag values that specify the location of the hook function and of the events to be skipped.</param>
    /// <returns>
    /// If successful, returns an HWINEVENTHOOK value that identifies this event hook instance. Applications save this return value to use it with the <see cref="UnhookWinEvent"/> function.
    /// If unsuccessful, returns zero.
    /// </returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SetWinEventHook(EventConstant eventMin, EventConstant eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    /// <summary>
    /// Removes an event hook function created by a previous call to <see cref="SetWinEventHook"/>.
    /// </summary>
    /// <remarks>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unhookwinevent"/>
    /// </remarks>
    /// <param name="hWinEventHook">Handle to the event hook returned in the previous call to SetWinEventHook.</param>
    /// <returns>If successful, returns TRUE; otherwise, returns FALSE.</returns>
    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    /// <summary>
    /// Retrieves the position of the mouse cursor, in screen coordinates.
    /// </summary>
	/// <remarks>
	/// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos"/>
	/// </remarks>
    /// <param name="lpPoint">A pointer to a <see cref="Point"/> structure that receives the screen coordinates of the cursor.</param>
    /// <returns>Returns nonzero if successful or zero otherwise. To get extended error information, call GetLastError.</returns>
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(ref Point lpPoint);

    /// <summary>
    ///     Retrieves a handle to the Shell's desktop window.
    ///     <para>
    ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633512%28v=vs.85%29.aspx for more
    ///     information
    ///     </para>
    /// </summary>
    /// <returns>
    ///     C++ ( Type: HWND )<br />The return value is the handle of the Shell's desktop window. If no Shell process is
    ///     present, the return value is NULL.
    /// </returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetShellWindow();

    #region Monitor Functions

    /// <summary>
    /// A MonitorEnumProc function is an application-defined callback function that is called by the EnumDisplayMonitors function.
    /// </summary>
    /// <remarks>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-monitorenumproc"/>
    /// </remarks>
    /// <param name="hMonitor">A handle to the display monitor. This value will always be non-NULL.</param>
    /// <param name="hdcMonitor">
    /// A handle to a device context.
    /// The device context has color attributes that are appropriate for the display monitor identified by hMonitor.The clipping area of the device context is set to the intersection of the visible region of the device context identified by the hdc parameter of EnumDisplayMonitors, the rectangle pointed to by the lprcClip parameter of EnumDisplayMonitors, and the display monitor rectangle.
    /// This value is NULL if the hdc parameter of EnumDisplayMonitors was NULL.
    /// </param>
    /// <param name="lprcMonitor">
    /// A pointer to a RECT structure.
    /// If hdcMonitor is non-NULL, this rectangle is the intersection of the clipping area of the device context identified by hdcMonitor and the display monitor rectangle.The rectangle coordinates are device-context coordinates.
    /// If hdcMonitor is NULL, this rectangle is the display monitor rectangle. The rectangle coordinates are virtual-screen coordinates.
    /// </param>
    /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the enumeration function.</param>
    /// <returns>
    /// To continue the enumeration, return TRUE.
    /// To stop the enumeration, return FALSE.
    /// </returns>
    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    /// <summary>
    /// The EnumDisplayMonitors function enumerates display monitors (including invisible pseudo-monitors associated with the mirroring drivers) that intersect a region formed by the intersection of a specified clipping rectangle and the visible region of a device context. EnumDisplayMonitors calls an application-defined MonitorEnumProc callback function once for each monitor that is enumerated. Note that GetSystemMetrics (SM_CMONITORS) counts only the display monitors.
    /// </summary>
    /// <remarks>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors"/>
    /// </remarks>
    /// <param name="hdc">
    /// A handle to a display device context that defines the visible region of interest.
    /// If this parameter is NULL, the hdcMonitor parameter passed to the callback function will be NULL, and the visible region of interest is the virtual screen that encompasses all the displays on the desktop.
    /// </param>
    /// <param name="lprcClip">
    /// A pointer to a RECT structure that specifies a clipping rectangle. The region of interest is the intersection of the clipping rectangle with the visible region specified by hdc.
    /// If hdc is non-NULL, the coordinates of the clipping rectangle are relative to the origin of the hdc.If hdc is NULL, the coordinates are virtual-screen coordinates.
    /// This parameter can be NULL if you don't want to clip the region specified by hdc.
    /// </param>
    /// <param name="lpfnEnum">A pointer to a MonitorEnumProc application-defined callback function.</param>
    /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the MonitorEnumProc function.</param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </returns>
    [DllImport("user32.dll")]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    public static IntPtr[] EnumDisplayMonitors()
    {
        var hMonitors = new List<IntPtr>();
        EnumDisplayMonitors(
            hdc: IntPtr.Zero,
            lprcClip: IntPtr.Zero,
            lpfnEnum: (IntPtr hMonitor, IntPtr _, ref Rect lprcMonitor, IntPtr _) => { hMonitors.Add(hMonitor); return true; },
            dwData: IntPtr.Zero);
        return hMonitors.ToArray();
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, MonitorFlags dwFlags);

    #endregion

    #region Window Functions

    internal static HWND[] EnumWindows()
    {
        var hWnds = new List<HWND>();
        PInvoke.EnumWindows((hWnd, lParam) => { hWnds.Add(hWnd); return true; }, new(0));
        return hWnds.ToArray();
    }

    /// <summary>
    /// <see href="https://www.pinvoke.net/default.aspx/user32.getwindowlong"/>
    /// </summary>
    /// <param name="hWnd">Window handle</param>
    /// <param name="nIndex"><see cref="WindowLongFlags"/></param>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")] // 64-bit only
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWNd, StringBuilder lpString, int nMaxCount);

    public static string GetWindowText(IntPtr hWnd, int maxLength)
    {
        var stringBuilder = new StringBuilder(maxLength);
        _ = GetWindowText(hWnd, stringBuilder, maxLength);
        return stringBuilder.ToString();
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public static string GetClassName(IntPtr hWnd, int maxLength)
    {
        var stringBuilder = new StringBuilder(maxLength);
        _ = GetClassName(hWnd, stringBuilder, maxLength);
        return stringBuilder.ToString();
    }

    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    /// <see href="https://www.pinvoke.net/default.aspx/user32.getancestor"/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags);

    /// <summary>
    /// <see href="https://www.pinvoke.net/default.aspx/user32.getlastactivepopup"/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetLastActivePopup(IntPtr hWnd);

    /// <summary>
    /// <see href="https://devblogs.microsoft.com/oldnewthing/20071008-00/?p=24863"/>
    /// </summary>
    /// <param name="hWnd">Handle of the window.</param>
    /// <returns></returns>
    public static bool IsAltTabWindow(IntPtr hWnd)
    {
        var hWndWalk = User32.GetAncestor(hWnd, GetAncestorFlags.GA_ROOTOWNER);
        IntPtr hWndTry;
        while ((hWndTry = GetLastActivePopup(hWndWalk)) != hWndTry)
        {
            // if (IsWindowVisible(hWndTry))
            //     break;
            hWndWalk = hWndTry;
        }
        return hWndWalk == hWnd;
    }

    /// <summary>
    /// Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window.
    /// <see href="https://www.pinvoke.net/default.aspx/user32/GetWindow.html"/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="uCmd"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsZoomed(IntPtr hWnd);

    /// <summary>
    /// <see href="https://www.pinvoke.net/default.aspx/user32/getwindowrect.html"/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="lpRect"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    /// <summary>
    /// <see href="https://www.pinvoke.net/default.aspx/user32.getclientrect"/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="lpRect"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos"/>
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="hWndInsertAfter"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="cx"></param>
    /// <param name="cy"></param>
    /// <param name="uFlags"></param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, HwndInsertAfterFlags hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags uFlags);

    #endregion

}