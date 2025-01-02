using System;
using cswm.Core.Layout;
using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.macOS.Services;

public class MacOsWindowController : IWindowController
{
    public MonitorInfo FetchMonitorInfo(IntPtr handle)
    {
        throw new NotImplementedException();
    }

    public MonitorInfo[] GetMonitors()
    {
        throw new NotImplementedException();
    }

    public WindowInfo FetchWindowInfo(IntPtr handle)
    {
        throw new NotImplementedException();
    }

    public WindowInfo[] GetWindows()
    {
        throw new NotImplementedException();
    }

    public void MoveWindow(IntPtr handle, Rect area)
    {
        throw new NotImplementedException();
    }
}