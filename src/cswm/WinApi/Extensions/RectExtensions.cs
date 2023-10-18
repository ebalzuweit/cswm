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
            ? (new(rect.Left, rect.Top, splitPosition, rect.Bottom), // left
                new(splitPosition, rect.Top, rect.Right, rect.Bottom)) // right
            : (new(rect.Left, rect.Top, rect.Right, splitPosition), // top
                new(rect.Left, splitPosition, rect.Right, rect.Bottom)); // bottom
    }

    public static Rect AddMargin(this Rect rect, int margin)
        => rect.AddMargin(margin, margin, margin, margin);

    public static Rect AddMargin(this Rect rect, int left, int top, int right, int bottom)
        => new(rect.Left + left, rect.Top + top, rect.Right - right, rect.Bottom - bottom);

    /// <summary>
    /// Adjust for padding added by Windows.
    /// </summary>
    /// <remarks>
    /// Credit: <see href="https://www.forrestthewoods.com/blog/building_a_better_aerosnap/"/>
    /// </remarks>
    /// <param name="rect">Desired position.</param>
    /// <param name="window">Window to be moved.</param>
    /// <returns>Adjusted position for SetWindowPos.</returns>
    public static Rect AdjustForWindowsPadding(this Rect rect, Window window)
    {
        var padding = (window.Position.Width - window.ClientPosition.Width) / 2;
        var adjusted = new Rect(
            left: rect.Left - padding,
            top: rect.Top,
            right: rect.Right + padding,
            bottom: rect.Bottom + padding);

        return adjusted;
    }
}