using System;
using cswm.WinApi;

namespace cswm.WindowManagement.Tracking;

public class DefaultWindowTrackingStrategy : IWindowTrackingStrategy
{
    public bool ShouldTrack(Window window) => IsAltTabWindow(window);

    private bool IsAltTabWindow(Window window)
    {
        if (window.hWnd == User32.GetWindow(window.hWnd, GetWindowType.GW_OWNER))
            return false;

        var root = User32.GetAncestor(window.hWnd, GetAncestorFlags.GA_ROOTOWNER);
        if (GetLastVisibleActivePopup(root) == window.hWnd)
        {
            var exstyle = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_EXSTYLE);
            if (HasExStyle(exstyle, ExtendedWindowStyle.WS_EX_APPWINDOW))
                return true;
            if (HasExStyle(exstyle, ExtendedWindowStyle.WS_EX_TOOLWINDOW))
                return false;
            if (HasExStyle(exstyle, ExtendedWindowStyle.WS_EX_NOACTIVATE))
                return false;


            return true;
        }
        return false;

        bool HasExStyle(long style, ExtendedWindowStyle ws) => (style & (long)ws) != 0;
    }

    private IntPtr GetLastVisibleActivePopup(IntPtr hwnd)
    {
        var last = User32.GetLastActivePopup(hwnd);
        if (User32.IsWindowVisible(last)) // TODO: DWMWA_CLOAKED == false
            return last;
        else if (last == hwnd)
            return IntPtr.Zero;
        else
            return GetLastVisibleActivePopup(last);
    }
}
