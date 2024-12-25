using System;
using cswm.Core.Layout;

namespace cswm.Core.Windows;

public sealed record MonitorInfo
(
	IntPtr Handle,
	Rect Bounds
);
