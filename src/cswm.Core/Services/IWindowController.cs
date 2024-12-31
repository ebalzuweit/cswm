using System;
using cswm.Core.Layout;
using cswm.Core.Models;

namespace cswm.Core.Services;

public interface IWindowController
{
	MonitorInfo FetchMonitorInfo(IntPtr handle);
	MonitorInfo[] GetMonitors();
	WindowInfo FetchWindowInfo(IntPtr handle);
	WindowInfo[] GetWindows();
	void MoveWindow(IntPtr handle, Rect area);
}
