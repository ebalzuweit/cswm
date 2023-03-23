using System;
using cswm.WinApi;

namespace cswm.WindowManagement.Arrangement;

public record WindowLayout(IntPtr hWnd, Rect Position);