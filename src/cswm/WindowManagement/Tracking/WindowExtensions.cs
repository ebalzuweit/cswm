using cswm.WinApi;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace cswm.WindowManagement.Tracking;

public static class WindowExtensions
{
    private static readonly Lazy<ImmutableArray<CachedStyle>> _styleCache =
        new Lazy<ImmutableArray<CachedStyle>>(() =>
        {
            var cache = new List<CachedStyle>();
            foreach (var ws in Enum.GetValues<WindowStyle>())
                cache.Add(new(ws));
            foreach (var exws in Enum.GetValues<ExtendedWindowStyle>())
                cache.Add(new(exws));

            return cache.ToImmutableArray();
        });

    public static string GetDebugString(this Window window)
    {
        var style = (long)User32.GetWindowLongPtr(window.hWnd, WindowLongFlags.GWL_STYLE);

        var sb = new StringBuilder();
        sb.AppendLine(window.ToString());
        sb.AppendLine($"Owner null: {User32.GetWindow(window.hWnd, GetWindowType.GW_OWNER)}");
        foreach (var (s, t) in _styleCache.Value)
        {
            sb.AppendLine($"{s}: {HasStyle(t)}");
        }
        return sb.ToString();

        bool HasStyle(long ws) => (style & ws) != 0;
    }

    private record CachedStyle(string Style, long Value)
    {
        public CachedStyle(WindowStyle ws) : this(ws.ToString(), (long)ws) { }
        public CachedStyle(ExtendedWindowStyle exws) : this(exws.ToString(), (long)exws) { }
    }
}
