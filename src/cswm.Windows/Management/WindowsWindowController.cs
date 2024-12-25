using System;
using cswm.Core.Layout;
using cswm.Core.Management;
using cswm.Core.Windows;

namespace cswm.Windows.Management;

public class WindowsWindowController : IWindowController
{
	public MonitorInfo FetchMonitorInfo(nint handle)
	{
		throw new NotImplementedException();
	}

	public WindowInfo FetchWindowInfo(nint handle)
	{
		throw new NotImplementedException();
	}

	public MonitorInfo[] GetMonitors()
	{
		throw new NotImplementedException();
	}

	public WindowInfo[] GetWindows()
	{
		throw new NotImplementedException();
	}

	public void MoveWindow(WindowInfo window, Rect area)
	{
		throw new NotImplementedException();
	}
}
