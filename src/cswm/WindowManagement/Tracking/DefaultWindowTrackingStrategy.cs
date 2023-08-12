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
            // Window must be min/max-imizable
            var style = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_STYLE);
            if (HasStyle(style, WindowStyle.WS_MAXIMIZEBOX) == false)
                return false;
            if (HasStyle(style, WindowStyle.WS_MINIMIZEBOX) == false)
                return false;

            // Follow extended window styling rules
            var exstyle = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_EXSTYLE);
            if (HasExStyle(exstyle, ExtendedWindowStyle.WS_EX_APPWINDOW))
                return true;
            if (HasExStyle(exstyle, ExtendedWindowStyle.WS_EX_TOOLWINDOW))
                return false;
            if (HasExStyle(exstyle, ExtendedWindowStyle.WS_EX_NOACTIVATE))
                return false;

            // TODO: Can we update logic to catch this exception?
            if (window.ClassName == "ApplicationFrameWindow")
                return false;

            return true;
        }
        return false;

        bool HasStyle(long style, WindowStyle ws) => (style & (long)ws) != 0;
        bool HasExStyle(long style, ExtendedWindowStyle ws) => (style & (long)ws) != 0;
    }

    private IntPtr GetLastVisibleActivePopup(IntPtr hwnd)
    {
        var last = User32.GetLastActivePopup(hwnd);
        if (User32.IsWindowVisible(last))
            return last;
        else if (last == hwnd)
            return IntPtr.Zero;
        else
            return GetLastVisibleActivePopup(last);
    }
}
