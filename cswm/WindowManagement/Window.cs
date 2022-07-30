using System;

namespace cswm.WindowManagement;

public class Window
{
    public IntPtr hWnd { get; init; }

    public Window(IntPtr hWnd)
    {
        this.hWnd = hWnd;
    }

    public override string ToString()
    {
        return $"[{hWnd}]";
    }
}