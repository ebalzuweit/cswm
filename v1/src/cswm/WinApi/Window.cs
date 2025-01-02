using System;
using Windows.Win32.Foundation;

namespace cswm.WinApi;

public class Window
{
    private const int WINDOW_CLASS_NAME_LENGTH = 256;
    private const int WINDOW_CAPTION_TEXT_LENGTH = 255;

    public HWND hWnd { get; init; }
    public string? Caption { get; init; }
    public string? ClassName { get; init; }
    public Rect Position { get; init; }
    public Rect ClientPosition { get; init; }

    public static Window FromHwnd(HWND hwnd)
    {
        Rect position, clientPosition;
        User32.GetWindowRect(hwnd, out position);
        User32.GetClientRect(hwnd, out clientPosition);

        return new Window()
        {
            hWnd = hwnd,
            Caption = User32.GetWindowText(hwnd, WINDOW_CAPTION_TEXT_LENGTH),
            ClassName = User32.GetClassName(hwnd, WINDOW_CLASS_NAME_LENGTH),
            Position = position,
            ClientPosition = clientPosition
        };
    }

    public override string ToString()
    {
        return $"{ClassName} : {Caption?.Substring(0, Math.Min(Caption.Length, 24))} [{hWnd.Value}] @ {Position}";
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