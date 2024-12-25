using System;
using cswm.Core.Models;

namespace cswm.Core.Services;

public interface IWindowEventHook : IDisposable
{
	event EventHandler<WindowInfo>? WindowCreated;
	event EventHandler<WindowInfo>? WindowDestroyed;
	event EventHandler<WindowInfo>? WindowMoved;
	event EventHandler<WindowInfo>? WindowStateChanged;

	void StartSubscriptions();
	void StopSubscriptions();
}
