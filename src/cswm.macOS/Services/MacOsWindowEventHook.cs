using System;
using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.macOS.Services;

public class MacOsWindowEventHook : IWindowEventHook
{
    public event EventHandler<WindowInfo>? WindowCreated;
    public event EventHandler<WindowInfo>? WindowDestroyed;
    public event EventHandler<WindowInfo>? WindowMoved;
    public event EventHandler<WindowInfo>? WindowStateChanged;
    
    public void StartSubscriptions()
    {
        throw new NotImplementedException();
    }

    public void StopSubscriptions()
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        StopSubscriptions();
    }
}