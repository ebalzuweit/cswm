using System;
using cswm.Core.Layout;

namespace cswm.Core.Models;

public sealed record WindowInfo(
	IntPtr Handle,
	string Title,
	string ProcessName,
	Rect Bounds,
	bool IsMinimized,
	bool IsMaximized
);