using System;
using System.Runtime.InteropServices;

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
}