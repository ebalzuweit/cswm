using System;
using System.ComponentModel;

namespace cswm.WinApi;

public static class RectExtensions
{
    public static bool Overlaps(this Rect rect, Rect other)
    {
        if (rect.Left >= other.Right || rect.Right <= other.Left ||
            rect.Top >= other.Bottom || rect.Bottom <= other.Top)
            return false;
        return true;
    }

    public static ValueTuple<Rect, Rect, bool> Split(this Rect rect, int margin = 0)
    {
        var verticalSplit = rect.Width >= rect.Height; // slightly prefer vertical splits ( LEFT | RIGHT ) to horizontal ( TOP | BOTTOM )
        var dimension = verticalSplit ? rect.Width : rect.Height;
        var midpoint = dimension / 2;
        if (dimension % 2 == 1)
            midpoint += 1; // left split gets the extra
        var leftMid = rect.Left + midpoint;
        var topMid = rect.Top + midpoint;
        // we give the left partition the extra 1/2 margin
        return verticalSplit
            ? new(
                new Rect(rect.Left, rect.Top, leftMid, rect.Bottom).AddMargin(margin, margin, 0, margin), // left partition
                new Rect(leftMid, rect.Top, rect.Right, rect.Bottom).AddMargin(margin), // right partition
                verticalSplit
            )
            : new(
                new Rect(rect.Left, rect.Top, rect.Right, topMid).AddMargin(margin, margin, margin, 0), // top partition
                new Rect(rect.Left, topMid, rect.Right, rect.Bottom).AddMargin(margin), // bottom partition
                verticalSplit
            );
    }

    public static Rect AddMargin(this Rect rect, int margin)
        => rect.AddMargin(margin, margin, margin, margin);

    public static Rect AddMargin(this Rect rect, int left, int top, int right, int bottom)
        => new(rect.Left + left, rect.Top + top, rect.Right - right, rect.Bottom - bottom);

    public static Rect AddPadding(this Rect rect, int padding)
        => rect.AddPadding(padding, padding, padding, padding);

    public static Rect AddPadding(this Rect rect, int left, int top, int right, int bottom)
        => new(rect.Left - left, rect.Top - top, rect.Right + right, rect.Bottom + bottom);
}