using System;
using cswm.Core.Layout;

public sealed record WindowInfo(
	IntPtr Handle,
	string Title,
	string ProcessName,
	Bounds Bounds,
	bool IsMinimized,
	bool IsMaximized
);