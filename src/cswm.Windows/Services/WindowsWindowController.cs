using System;
using cswm.Core.Layout;
using cswm.Core.Services;
using cswm.Core.Models;

namespace cswm.Windows.Services;

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
