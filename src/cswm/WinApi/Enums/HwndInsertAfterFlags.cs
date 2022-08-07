namespace cswm.WinApi;

public enum HwndInsertAfterFlags : int
{
    /// <summary>
    /// Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
    /// </summary>
    HWND_NOTOPMOST = -2,
}