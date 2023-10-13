using System;
using System.Runtime.InteropServices;

namespace cswm.WinApi;

public static class DwmApi
{
    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hWnd, DwmWindowAttribute dwAttribute, out bool pvAttribute, int cbAttribute);

    public static bool IsWindowVisible(Window window)
    {
        var isVisible = User32.IsWindowVisible(window.hWnd);
        if (isVisible == false)
            return false;

        _ = DwmApi.DwmGetWindowAttribute(window.hWnd, DwmWindowAttribute.DWMWA_CLOAKED, out var isCloaked, Marshal.SizeOf<bool>());
        if (isCloaked)
            return false;

        return true;
    }
}