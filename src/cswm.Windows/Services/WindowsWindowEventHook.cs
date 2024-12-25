using System;
using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.Windows.Services;

public class WindowsWindowEventHook : IWindowEventHook
{
	public event EventHandler<WindowInfo> WindowCreated;
	public event EventHandler<WindowInfo> WindowDestroyed;
	public event EventHandler<WindowInfo> WindowMoved;
	public event EventHandler<WindowInfo> WindowStateChanged;

	public void Dispose()
	{
		throw new NotImplementedException();
	}

	public void StartSubscriptions()
	{
		throw new NotImplementedException();
	}

	public void StopSubscriptions()
	{
		throw new NotImplementedException();
	}
}
