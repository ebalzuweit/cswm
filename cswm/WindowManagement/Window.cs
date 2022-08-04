using System;
using cswm.WinApi;

namespace cswm.WindowManagement;

public class Window
{
    private const int WINDOW_CLASS_NAME_LENGTH = 256;
    private const int WINDOW_CAPTION_TEXT_LENGTH = 255;

    public IntPtr hWnd { get; init; }
    public string Caption { get; init; }
    public string ClassName { get; init; }
    public Rect Position { get; init; }

    public Window(IntPtr hWnd)
    {
        this.hWnd = hWnd;
        Caption = User32.GetWindowText(hWnd, WINDOW_CAPTION_TEXT_LENGTH);
        ClassName = User32.GetClassName(hWnd, WINDOW_CLASS_NAME_LENGTH);
        if (User32.GetWindowRect(hWnd, out var lpRect))
            Position = lpRect;
    }

    public override string ToString()
    {
        return $"[{hWnd}] {ClassName} : {Caption.Substring(0, 24)} @ {Position}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        var window = obj as Window;
        if (window is null)
            return false;
        return this.hWnd == window.hWnd;
    }

    public override int GetHashCode()
    {
        return hWnd.GetHashCode();
    }
}