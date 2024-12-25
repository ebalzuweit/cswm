using System;
using cswm.Core.Layout;

namespace cswm.Core.Models;

public sealed record MonitorInfo
(
	IntPtr Handle,
	Rect Bounds
);
