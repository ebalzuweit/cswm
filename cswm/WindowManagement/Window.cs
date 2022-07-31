using System;
using cswm.WinApi;

namespace cswm.WindowManagement;

public class Window
{
    private const int WINDOW_CLASS_NAME_LENGTH = 20;
    private const int WINDOW_CAPTION_TEXT_LENGTH = 60;

    public IntPtr hWnd { get; init; }
    public string? Caption { get; init; }
    public string? ClassName { get; init; }

    public Window(IntPtr hWnd)
    {
        this.hWnd = hWnd;
        Caption = User32.GetWindowText(hWnd, WINDOW_CAPTION_TEXT_LENGTH);
        ClassName = User32.GetClassName(hWnd, WINDOW_CLASS_NAME_LENGTH);
    }

    public override string ToString()
    {
        return $"[{hWnd}] {ClassName} : {Caption}";
    }
}