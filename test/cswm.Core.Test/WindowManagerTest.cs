using cswm.Core.Models;
using cswm.Core.Test.Services;

namespace cswm.Core.Test;

public class WindowManagerTest
{
	private readonly MockWindowEventHook windowEventHook;
	private readonly WindowManager windowManager;

	public WindowManagerTest(
		MockWindowEventHook windowEventHook,
		WindowManager windowManager
	)
	{
		this.windowEventHook = windowEventHook;
		this.windowManager = windowManager;
	}

	[Fact]
	public void OnWindowCrated_AddWindowToLayout_IfWindowShouldBeManaged()
	{
		var window = new WindowInfo(0, string.Empty, string.Empty, new(0, 0, 1, 1), false, false);

		windowManager.Start();
		windowEventHook.InvokeWindowCreated(window);

		var windowsAfter = windowManager.GetWindows().ToArray();

		Assert.NotEmpty(windowsAfter);
		Assert.Contains(windowsAfter, x => x.Handle == window.Handle);
	}
}
