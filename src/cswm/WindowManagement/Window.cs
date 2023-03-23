using cswm.WinApi;
using System;
using Windows.Win32.Foundation;
namespace cswm.WindowManagement;

public class Window
{
    private const int WINDOW_CLASS_NAME_LENGTH = 256;
    private const int WINDOW_CAPTION_TEXT_LENGTH = 255;

    internal HWND hWnd { get; init; }
    public string Caption { get; init; }
    public string ClassName { get; init; }
    public Rect Position { get; init; }
    public Rect ClientPosition { get; init; }

    internal Window(HWND hWnd)
    {
        this.hWnd = hWnd;
        Caption = User32.GetWindowText(hWnd, WINDOW_CAPTION_TEXT_LENGTH);
        ClassName = User32.GetClassName(hWnd, WINDOW_CLASS_NAME_LENGTH);
        if (User32.GetWindowRect(hWnd, out var lpRect))
            Position = lpRect;
        if (User32.GetClientRect(hWnd, out var lpClientRect))
            ClientPosition = lpClientRect;
    }

    public override string ToString()
    {
        return $"{ClassName} : {Caption.Substring(0, Math.Min(Caption.Length, 24))} [{hWnd.Value}] @ {Position}";
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