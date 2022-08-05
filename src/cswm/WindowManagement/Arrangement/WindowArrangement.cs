using System;
using System.Collections.Generic;
using cswm.WinApi;

namespace cswm.WindowManagement.Arrangement;

public record WindowArrangement
{
    internal readonly IEnumerable<Window> _windows;
    internal readonly IEnumerable<Rect> _positions;

    public WindowArrangement(ICollection<Window> windows, ICollection<Rect> positions)
    {
        if (windows.Count != positions.Count)
            throw new ArgumentException("Handles and positions must have the same number of elements.");
        if (windows.Count == 0 || positions.Count == 0)
            throw new ArgumentException("Must pass at least one window and position.");

        _windows = windows;
        _positions = positions;
    }
}