using System.Runtime.InteropServices;

namespace cswm.WinApi;

/// <summary>
/// <see href="https://www.pinvoke.net/default.aspx/Structures/RECT.html"/>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Rect
{
    public readonly int Left, Top, Right, Bottom;

    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public int Area => Width * Height;

    public Rect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public override string ToString()
    {
        return $"[Rect:({Left},{Top},{Right},{Bottom})]";
    }
}