using System;
using System.Runtime.InteropServices;

namespace cswm.WinApi;

public static class DwmApi
{
    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hWnd, DwmWindowAttribute dwAttribute, out bool pvAttribute, int cbAttribute);
}