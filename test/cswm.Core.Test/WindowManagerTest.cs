using cswm.Core.Layout;
using cswm.Core.Layout.Engines;
using cswm.Core.Models;
using cswm.Core.Services;
using cswm.Core.Test.Services;

namespace cswm.Core.Test;

public class WindowManagerTest
{
	private readonly WindowRegistry windowRegistry;
	private readonly IWindowController windowController;
	private readonly MockWindowEventHook windowEventHook;
	private readonly WindowManager windowManager;

	public WindowManagerTest(
		WindowRegistry windowRegistry
	)
	{
		this.windowRegistry = windowRegistry;
		windowController = new MockWindowController(this.windowRegistry);
		windowEventHook = new MockWindowEventHook();
		windowManager = new(
			windowEventHook,
			windowController,
			new BspTilingLayoutEngine(),
			this.windowRegistry
		);
	}

	[Fact]
	public void OnStart_InitializeLayoutWithExistingWindow()
	{
		var monitor = windowController.GetMonitors().First();
		var window = GetWindowInfo();

		windowRegistry.RegisterWindow(window);

		windowManager.Start();

		var windowsAfter = windowManager.GetManagedWindows().ToArray();

		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x =>
			x.Handle == window.Handle &&
			x.Bounds == monitor.Bounds
		);
	}

	[Fact]
	public void OnWindowCreated_AddWindowToLayout_IfWindowShouldBeManaged()
	{
		var monitor = windowController.GetMonitors().First();
		var window = GetWindowInfo();

		windowManager.Start();

		var windowsBefore = windowManager.GetManagedWindows().ToArray();
		Assert.Empty(windowsBefore);

		windowEventHook.InvokeWindowCreated(window);

		var windowsAfter = windowManager.GetManagedWindows().ToArray();

		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x =>
			x.Handle == window.Handle &&
			x.Bounds == monitor.Bounds
		);
	}

	[Fact]
	public void OnWindowCreated_DoNothing_IfWindowMinimized()
	{
		var monitor = windowController.GetMonitors().First();
		var initialBounds = DefaultBounds;
		var window = GetWindowInfo(bounds: initialBounds, minimized: true);

		windowManager.Start();

		var windowsBefore = windowManager.GetAllWindows().ToArray();
		Assert.Empty(windowsBefore);

		windowEventHook.InvokeWindowCreated(window);

		var windowsAfter = windowManager.GetAllWindows().ToArray();

		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x =>
			x.Handle == window.Handle &&
			x.Bounds == initialBounds
		);
	}

	[Fact]
	public void OnWindowCreated_DoNothing_IfWindowMaximized()
	{
		var monitor = windowController.GetMonitors().First();
		var initialBounds = DefaultBounds;
		var window = GetWindowInfo(bounds: initialBounds, maximized: true);

		windowManager.Start();

		Assert.Empty(windowManager.GetAllWindows());

		windowEventHook.InvokeWindowCreated(window);

		var windowsAfter = windowManager.GetAllWindows().ToArray();

		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x =>
			x.Handle == window.Handle &&
			x.Bounds == initialBounds
		);
	}

	[Fact]
	public void OnWindowDestroyed_RemoveWindowFromLayout_IfWindowManaged()
	{
		var window = GetWindowInfo();
		windowRegistry.RegisterWindow(window);

		windowManager.Start();

		Assert.NotEmpty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowDestroyed(window);

		Assert.Empty(windowManager.GetManagedWindows());
	}

	[Fact]
	public void OnWindowDestroyed_DoNothing_IfWindowMinimized()
	{
		var window = GetWindowInfo(minimized: true);
		windowRegistry.RegisterWindow(window);

		windowManager.Start();

		Assert.NotEmpty(windowManager.GetAllWindows());
		Assert.Empty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowDestroyed(window);

		Assert.Empty(windowManager.GetAllWindows());
	}

	[Fact]
	public void OnWindowDestroyed_DoNothing_IfWindowMaximized()
	{
		var window = GetWindowInfo(maximized: true);
		windowRegistry.RegisterWindow(window);

		windowManager.Start();

		Assert.NotEmpty(windowManager.GetAllWindows());
		Assert.Empty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowDestroyed(window);

		Assert.Empty(windowManager.GetAllWindows());
	}

	[Fact]
	public void OnWindowMoved_SwapWindows_IfMovedOverWindow()
	{
		var monitor = windowController.GetMonitors().First();
		var split = monitor.Bounds.Left + (monitor.Bounds.Width / 2);
		var window0 = GetWindowInfo(handle: 0);
		var window1 = GetWindowInfo(handle: 1);
		windowRegistry.RegisterWindow(window0);
		windowRegistry.RegisterWindow(window1);
		windowManager.Start();

		var windowsBefore = windowManager.GetManagedWindows();
		Assert.NotEmpty(windowsBefore);
		Assert.Contains(windowsBefore, x =>
			x.Handle == window0.Handle &&
			x.Bounds == monitor.Bounds with { Right = split });
		Assert.Contains(windowsBefore, x =>
			x.Handle == window1.Handle &&
			x.Bounds == monitor.Bounds with { Left = split + 1 });

		// Update window info after initial layout
		window0 = windowRegistry.GetWindowInfo(window0.Handle);
		window1 = windowRegistry.GetWindowInfo(window1.Handle);
		Assert.NotNull(window0);
		Assert.NotNull(window1);

		// Move window0 to window1
		var left = window1.Bounds.Left + 1;
		var top = window1.Bounds.Top + 1;
		windowEventHook.InvokeWindowMoved(window0 with
		{
			Bounds = new Rect(left, top, left + window0.Bounds.Width, top + window0.Bounds.Height)
		});

		var windowsAfter = windowManager.GetManagedWindows();
		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x =>
			x.Handle == window0.Handle &&
			x.Bounds == monitor.Bounds with { Left = split + 1 });
		Assert.Contains(windowsAfter, x =>
			x.Handle == window1.Handle &&
			x.Bounds == monitor.Bounds with { Right = split });
	}

	[Fact(Skip = "TODO: Requires multiple monitor support.")]
	public void OnWindowMoved_SwapMonitors_IfMovedToMonitor()
	{
		Assert.Fail("TODO: Multiple monitor support");
	}

	[Fact]
	public void OnWindowMoved_DoNothing_IfWindowNotManaged()
	{
		var monitor = windowController.GetMonitors().First();
		var initialBounds = DefaultBounds;
		var updatedBounds = new Rect(1, 1, 2, 2);
		var window = GetWindowInfo(maximized: true);

		windowRegistry.RegisterWindow(window);

		windowManager.Start();

		Assert.NotEmpty(windowManager.GetAllWindows());
		Assert.Contains(windowManager.GetAllWindows(), x =>
			x.Handle == window.Handle &&
			x.Bounds == initialBounds);

		windowEventHook.InvokeWindowMoved(window with
		{
			Bounds = updatedBounds
		});

		Assert.NotEmpty(windowManager.GetAllWindows());
		Assert.Contains(windowManager.GetAllWindows(), x =>
			x.Handle == window.Handle &&
			x.Bounds == updatedBounds);
	}

	[Fact]
	public void OnWindowStateChanged_StopsManagingWindow_IfWindowMinimized()
	{
		var window = GetWindowInfo();
		windowRegistry.RegisterWindow(window);
		windowManager.Start();

		Assert.NotEmpty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowStateChanged(window with
		{
			IsMinimized = true
		});

		Assert.Empty(windowManager.GetManagedWindows());
	}

	[Fact]
	public void OnWindowStateChanged_StopsManagingWindow_IfWindowMaximized()
	{
		var window = GetWindowInfo();
		windowRegistry.RegisterWindow(window);
		windowManager.Start();

		Assert.NotEmpty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowStateChanged(window with
		{
			IsMaximized = true
		});

		Assert.Empty(windowManager.GetManagedWindows());
	}

	[Fact]
	public void OnWindowStateChanged_StartManagingWindow_IfWindowUnMinimized()
	{
		var window = GetWindowInfo(minimized: true);
		windowRegistry.RegisterWindow(window);
		windowManager.Start();

		Assert.Empty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowStateChanged(window with
		{
			IsMinimized = false
		});

		Assert.NotEmpty(windowManager.GetManagedWindows());
	}

	[Fact]
	public void OnWindowStateChanged_StartManagingWindow_IfWindowUnMaximized()
	{
		var window = GetWindowInfo(maximized: true);
		windowRegistry.RegisterWindow(window);
		windowManager.Start();

		Assert.Empty(windowManager.GetManagedWindows());

		windowEventHook.InvokeWindowStateChanged(window with
		{
			IsMaximized = false
		});

		Assert.NotEmpty(windowManager.GetManagedWindows());
	}

	#region Helper Methods

	private readonly Rect DefaultBounds = new Rect(0, 0, 1, 1);

	private WindowInfo GetWindowInfo(nint handle = 0, Rect? bounds = null, bool minimized = false, bool maximized = false)
		=> new WindowInfo(handle, string.Empty, string.Empty, bounds ?? DefaultBounds, minimized, maximized);

	#endregion
}
