using System;

namespace cswm.Core.Management;

public interface IWindowEventHook : IDisposable
{
	event EventHandler<WindowInfo> WindowCreated;
	event EventHandler<WindowInfo> WindowDestroyed;
	event EventHandler<WindowInfo> WindowMoved;
	event EventHandler<WindowInfo> WindowStateChanged;

	void StartSubscriptions();
	void StopSubscriptions();
}
