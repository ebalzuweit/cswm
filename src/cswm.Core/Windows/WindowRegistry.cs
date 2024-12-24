using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace cswm.Core.Windows;

public class WindowRegistry
{
	private readonly ILogger logger;
	internal readonly IDictionary<IntPtr, WindowInfo> registeredWindows = new Dictionary<IntPtr, WindowInfo>();

	public WindowRegistry(ILogger<WindowRegistry> logger)
	{
		this.logger = logger;
	}

	public void RegisterWindow(WindowInfo window)
	{
		ThrowIfWindowNull(window);

		logger.LogDebug("Registering window: [{handle}] {title}", window.Handle, window.Title);
		registeredWindows.Add(window.Handle, window);
	}

	public bool UnregisterWindow(WindowInfo window)
	{
		ThrowIfWindowNull(window);

		logger.LogDebug("Unregistering window: [{handle}] {title}", window.Handle, window.Title);
		return registeredWindows.Remove(window.Handle);
	}

	public WindowInfo? GetWindowInfo(IntPtr handle)
	{
		if (registeredWindows.TryGetValue(handle, out var windowInfo))
		{
			return windowInfo;
		}
		return null;
	}

	public void UpdateWindow(WindowInfo window)
	{
		ThrowIfWindowNull(window);

		if (registeredWindows.ContainsKey(window.Handle))
		{
			logger.LogDebug("Updating window registry: [{handle}] {title}", window.Handle, window.Title);
			registeredWindows[window.Handle] = window;
		}
		else
		{
			RegisterWindow(window);
		}
	}

	private void ThrowIfWindowNull(WindowInfo window)
	{
		if (window is null)
		{
			throw new InvalidOperationException($"{typeof(WindowInfo)} cannot be null!");
		}
	}
}
