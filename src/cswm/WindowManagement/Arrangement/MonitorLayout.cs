using System;
using System.Collections.Generic;
using cswm.WinApi;

namespace cswm.WindowManagement.Arrangement;

public record MonitorLayout(IntPtr hMon, Rect Space, IEnumerable<WindowLayout> Windows, float AspectRatio = 1.7777777778f);