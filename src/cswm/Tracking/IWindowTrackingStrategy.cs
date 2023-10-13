using cswm.WinApi;

namespace cswm.Tracking;

public interface IWindowTrackingStrategy
{
    /// <summary>
    /// Should the window be tracked or not.
    /// </summary>
    /// <remarks>
    /// WIndow has been checked as visible.
    /// </remarks>
    /// <param name="window">Window</param>
    /// <returns><c>true</c> if the window should be tracked; otherwise <c>false</c>.</returns>
    bool ShouldTrack(Window window);
}
