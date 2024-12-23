using cswm.WinApi;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace cswm.Tracking;

public class DefaultWindowTrackingStrategy : IWindowTrackingStrategy
{
    public bool ShouldTrack(Window window)
    {
        // no GW_OWNER
        var owner = PInvoke.GetWindow(window.hWnd, GET_WINDOW_CMD.GW_OWNER);
        if (owner == window.hWnd)
        {
            return false;
        }

        if (PInvoke.IsWindowVisible(window.hWnd) == false)
        {
            return false;
        }

        var style = PInvoke.GetWindowLong(window.hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        var exStyle = PInvoke.GetWindowLong(window.hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

        if (HasStyle(style, WindowStyle.WS_MAXIMIZEBOX) == false || HasStyle(style, WindowStyle.WS_MINIMIZEBOX) == false)
        {
            return false;
        }

        // Note: All exclusion rules must be above this line

        // respect extended styling rules
        if (HasExStyle(exStyle, ExtendedWindowStyle.WS_EX_TOOLWINDOW) || HasExStyle(exStyle, ExtendedWindowStyle.WS_EX_NOACTIVATE))
        {
            return false;
        }
        if (HasExStyle(exStyle, ExtendedWindowStyle.WS_EX_APPWINDOW))
        {
            return true;
        }

        if (HasStyle(style, WindowStyle.WS_POPUP) || HasExStyle(exStyle, ExtendedWindowStyle.WS_EX_CLIENTEDGE) || HasExStyle(exStyle, 0x00000001 /* WS_EX_DLGMODALFRAME */))
        {
            return false;
        }

        // Top-level window
        if (window.hWnd == PInvoke.GetAncestor(window.hWnd, GET_ANCESTOR_FLAGS.GA_ROOT))
        {
            return true;
        }

        // WS_OVERLAPPEDWINDOW suggests true, but some windows lie
        //if (HasStyle(style, WindowStyle.WS_OVERLAPPEDWINDOW))
        //{
        //    return true;
        //}

        return false;
    }

    private static bool HasStyle(int style, WindowStyle ws) => (style & (int)ws) != 0;
    private static bool HasExStyle(int exStyle, ExtendedWindowStyle ws) => (exStyle & (int)ws) != 0;
    private static bool HasExStyle(int exStyle, int ws) => (exStyle & ws) != 0;

}
