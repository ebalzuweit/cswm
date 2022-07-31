using System;
using System.Text;
using cswm.WinApi;

namespace cswm.WindowManagement;

public class Window
{
    public IntPtr hWnd { get; init; }
    public string? Caption { get; init; }

    public Window(IntPtr hWnd)
    {
        this.hWnd = hWnd;
        var sb = new StringBuilder(40);
        _ = User32.GetWindowText(hWnd, sb, 40);
        Caption = sb.ToString();
    }

    public bool HasStyle(WindowStyle style)
    {
        var windowStyles = (long)User32.GetWindowLongPtr(hWnd, WindowLongFlags.GWL_STYLE);
        return (windowStyles & (long)style) != 0;
    }

    public override string ToString()
    {
        return $"[{hWnd}] {Caption}";
    }
}