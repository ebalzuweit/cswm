using cswm.WinApi;
using System;
using System.Collections.Generic;

namespace cswm.WindowManagement.Arrangement;

public record MonitorLayout(IntPtr MonitorHandle, Rect Space, IEnumerable<WindowLayout> Windows, float AspectRatio = 1.7777777778f);