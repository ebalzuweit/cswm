using cswm.WinApi;

namespace cswm.WindowManagement.Tracking;

public class DefaultWindowTrackingStrategy : IWindowTrackingStrategy
{
    public bool ShouldTrack(Window window)
    {
        if (User32.IsIconic(window.hWnd))
            return false;

        var style = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_STYLE);
        if (HasStyle(WindowStyle.WS_CAPTION) == false ||
            HasStyle(WindowStyle.WS_MINIMIZEBOX) == false ||
            HasStyle(WindowStyle.WS_MAXIMIZEBOX) == false)
            return false;
        if (HasStyle(WindowStyle.WS_CLIPCHILDREN) == false &&
            HasStyle(WindowStyle.WS_POPUP))
            return false;

        return true;

        bool HasStyle(WindowStyle ws) => (style & (long)ws) != 0;
    }
}
