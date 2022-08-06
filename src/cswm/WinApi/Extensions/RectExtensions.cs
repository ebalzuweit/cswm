namespace cswm.WinApi;

public static class RectExtensions
{
    public static bool Overlaps(this Rect rect, Rect other)
    {
        if (rect.Left > other.Right || rect.Right < other.Left ||
            rect.Top > other.Bottom || rect.Bottom < other.Top)
            return false;
        return true;
    }
}