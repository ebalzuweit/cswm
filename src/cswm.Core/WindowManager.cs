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

		RelayoutAllMonitors();

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

	public IEnumerable<WindowInfo> GetAllWindows()
	{
		return windowRegistry.GetAllWindows();
	}

	public IEnumerable<WindowInfo> GetManagedWindows()
	{
		return windowRegistry
			.GetAllWindows()
			.Where(ShouldManageWindow);
	}

	private bool ShouldManageWindow(WindowInfo window)
		=> window.IsMinimized == false && window.IsMaximized == false;

	private void OnWindowCreated(object? sender, WindowInfo window)
	{
		windowRegistry.RegisterWindow(window);
		// HACK: Only need to relayout the relevant monitor
		RelayoutAllMonitors();
	}

	private void OnWindowDestroyed(object? sender, WindowInfo window)
	{
		windowRegistry.UnregisterWindow(window);
		// HACK: Only need to relayout the relevant monitor
		RelayoutAllMonitors();
	}

	private void OnWindowMoved(object? sender, WindowInfo window)
	{
		windowRegistry.UpdateWindow(window);
		// TODO: Handle monitor/window swapping
		RelayoutAllMonitors(window.Handle);
	}

	private void OnWindowStateChanged(object? sender, WindowInfo window)
	{
		windowRegistry.UpdateWindow(window);
		// HACK: Only need to relayout the relevant monitor
		RelayoutAllMonitors();
	}

	private void RelayoutAllMonitors(nint? priorityWindowHandle = null)
	{
		// TODO: Multi-monitor support
		var monitor = windowController.GetMonitors().First();
		var windows = GetManagedWindows();
		var monitorLayout = layoutEngine.CalculateLayout(monitor.Bounds, windows, priorityWindowHandle);
		foreach (var windowLayout in monitorLayout.WindowLayouts)
		{
			windowController.MoveWindow(windowLayout.Handle, windowLayout.Area);
		}
	}
}
