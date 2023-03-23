using cswm.WinApi;
using System;

namespace cswm.WindowManagement.Arrangement;

public record WindowLayout(IntPtr hWnd, Rect Position);