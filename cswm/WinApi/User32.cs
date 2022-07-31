using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace cswm.WinApi;

public static class User32
{
    public delegate void WinEventProc(IntPtr hWinEventHook, EventConstant eventType, IntPtr hwnd, ObjectIdentifier idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    /// <summary>
    /// <see href="https://www.pinvoke.net/default.aspx/user32.setwineventhook"/>
    /// </summary>
    /// <param name="eventMin"></param>
    /// <param name="eventMax"></param>
    /// <param name="hmodWinEventProc"></param>
    /// <param name="lpfnWinEventProc"></param>
    /// <param name="idProcess"></param>
    /// <param name="idThread"></param>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern IntPtr SetWinEventHook(EventConstant eventMin, EventConstant eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    public static IntPtr[] EnumWindows()
    {
        var hWnds = new List<IntPtr>();
        EnumWindows((hWnd, lParam) => { hWnds.Add(hWnd); return true; }, IntPtr.Zero);
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
}