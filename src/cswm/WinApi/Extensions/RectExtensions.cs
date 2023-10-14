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
}