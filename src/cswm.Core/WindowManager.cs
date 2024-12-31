using System.Collections.Generic;
using System.Linq;
using cswm.Core.Layout;
using cswm.Core.Models;
using cswm.Core.Services;

namespace cswm.Core;

public class WindowManager
{
	private readonly IWindowEventHook windowEventHook;
	private readonly IWindowController windowController;
	private readonly ILayoutEngine layoutEngine;
	private readonly WindowRegistry windowRegistry;

	public WindowManager(
		IWindowEventHook windowEventHook,
		IWindowController windowController,
		ILayoutEngine layoutEngine,
		WindowRegistry windowRegistry
	)
	{
		this.windowEventHook = windowEventHook;
		this.windowController = windowController;
		this.layoutEngine = layoutEngine;
		this.windowRegistry = windowRegistry;
	}

	public void Start()
	{
		windowEventHook.WindowCreated += OnWindowCreated;
		windowEventHook.WindowDestroyed += OnWindowDestroyed;
		windowEventHook.WindowMoved += OnWindowMoved;
		windowEventHook.WindowStateChanged += OnWindowStateChanged;

		windowEventHook.StartSubscriptions();
	}

	public void Stop()
	{
		windowEventHook.WindowCreated -= OnWindowCreated;
		windowEventHook.WindowDestroyed -= OnWindowDestroyed;
		windowEventHook.WindowMoved -= OnWindowMoved;
		windowEventHook.WindowStateChanged -= OnWindowStateChanged;

		windowEventHook.StopSubscriptions();
	}

	public IEnumerable<WindowInfo> GetWindows()
	{
		return windowRegistry.GetAllWindows();
	}

	private void OnWindowCreated(object? sender, WindowInfo window)
	{
		windowRegistry.RegisterWindow(window);
		// TODO: Multi-monitor support
		var monitor = windowController.GetMonitors().First();
		var monitorLayout = layoutEngine.CalculateLayout(monitor.Bounds, GetWindows());
		foreach (var windowLayout in monitorLayout.WindowLayouts)
		{
			windowController.MoveWindow(windowLayout.Handle, windowLayout.Area);
		}
	}

	private void OnWindowDestroyed(object? sender, WindowInfo window)
	{
		windowRegistry.UnregisterWindow(window);
		// TODO: Update layout
	}

	private void OnWindowMoved(object? sender, WindowInfo window)
	{
		// TODO: Update layout
	}

	private void OnWindowStateChanged(object? sender, WindowInfo window)
	{
		// TODO: Update layout
	}
}
