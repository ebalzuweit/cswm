using cswm.WinApi;
using System;
using System.Collections.Immutable;
using System.Text;

namespace cswm.WindowManagement.Tracking;

public static class WindowExtensions
{
    private static readonly Lazy<ImmutableDictionary<string, WindowStyle>> _styleCache =
        new Lazy<ImmutableDictionary<string, WindowStyle>>(() =>
            Enum.GetValues<WindowStyle>().ToImmutableDictionary(x => x.ToString()));

    public static string GetDebugString(this Window window)
    {
        var style = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_STYLE);

        var sb = new StringBuilder();
        sb.AppendLine(window.ToString());
        foreach (var (s, t) in _styleCache.Value)
        {
            sb.AppendLine($"{s}: {HasStyle(t)}");
        }
        return sb.ToString();

        bool HasStyle(WindowStyle ws) => (style & (long)ws) != 0;
    }
}
