using System;

namespace cswm.WinApi;

public static class RectExtensions
{
    public static bool Intersects(this Rect a, Rect b)
    {
        if (a.Left >= b.Right || a.Right <= b.Left ||
            a.Top >= b.Bottom || a.Bottom <= b.Top)
            return false;
        return true;
    }

    public static float IntersectionAreaPct(this Rect a, Rect b)
    {
        if (a.Intersects(b) == false)
            return 0;

        return Math.Min(a.Right - b.Left, b.Right - a.Left) * Math.Min(a.Bottom - b.Top, b.Bottom - a.Top);
        //var intersection =
        //    Math.Max(0, Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left)) *
        //    Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));
        //var union = a.Area + b.Area - intersection;

        //return intersection / (float)union;
    }

    public static (Rect Left, Rect Right, bool VerticalSplit) Split(this Rect rect, int margin = 0)
    {
        var aspectRatio = rect.Width / (float)rect.Height;
        var verticalSplit = aspectRatio >= 1.33; // slightly prefer vertical splits ( LEFT | RIGHT ) to horizontal ( TOP | BOTTOM )
        var dimension = verticalSplit ? rect.Width : rect.Height;
        var midpoint = dimension / 2;
        if (dimension % 2 == 1)
            midpoint += 1; // left split gets the extra
        var leftMid = rect.Left + midpoint;
        var topMid = rect.Top + midpoint;
        // we give the left partition the extra 1/2 margin
        return verticalSplit
            ? (
                new Rect(rect.Left, rect.Top, leftMid, rect.Bottom).AddMargin(margin, margin, 0, margin), // left partition
                new Rect(leftMid, rect.Top, rect.Right, rect.Bottom).AddMargin(margin), // right partition
                verticalSplit
            )
            : (
                new Rect(rect.Left, rect.Top, rect.Right, topMid).AddMargin(margin, margin, margin, 0), // top partition
                new Rect(rect.Left, topMid, rect.Right, rect.Bottom).AddMargin(margin), // bottom partition
                verticalSplit
            );
    }

    public static (Rect Left, Rect Right) SplitAt(this Rect rect, bool verticalSplit, int splitPosition)
    {
        return verticalSplit
            ? (new(rect.Left, rect.Top, rect.Left + splitPosition, rect.Bottom), // left
                new(rect.Left + splitPosition, rect.Top, rect.Right, rect.Bottom)) // right
            : (new(rect.Left, rect.Top, rect.Right, rect.Top + splitPosition), // top
                new(rect.Left, rect.Top + splitPosition, rect.Right, rect.Bottom)); // bottom
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