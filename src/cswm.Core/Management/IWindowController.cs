using System;
using cswm.Core.Layout;
using cswm.Core.Windows;

namespace cswm.Core.Management;

public interface IWindowController
{
	MonitorInfo FetchMonitorInfo(IntPtr handle);
	MonitorInfo[] GetMonitors();
	WindowInfo FetchWindowInfo(IntPtr handle);
	WindowInfo[] GetWindows();
	void MoveWindow(WindowInfo window, Rect area);
}
