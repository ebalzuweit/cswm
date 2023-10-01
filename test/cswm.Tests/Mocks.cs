using cswm.WinApi;
using cswm.WindowManagement;
using cswm.WindowManagement.Arrangement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;

namespace cswm.Tests;

public static class Mocks
{
    public static ILogger<T> Logger<T>()
    {
        var mock = new Mock<ILogger<T>>();
        return mock.Object;
    }

    public static Monitor[] GetMonitors(Rect size, int count = 1)
    {
        var monitors = new Monitor[count];
        for (int i = 0; i < count; i++)
        {
            var mock = new Mock<Monitor>(IntPtr.Zero);
            mock.Setup(monitor => monitor.WorkArea)
                .Returns(size);
            monitors[i] = mock.Object;
        }
        return monitors;
    }

    public static WindowLayout[] GetWindowLayouts(int count = 1)
    {
        var windows = new WindowLayout[count];
        for (int i = 0; i < count; i++)
        {
            windows[i] = new WindowLayout(new Window((Windows.Win32.Foundation.HWND)IntPtr.Zero), new Rect());
        }
        return windows;
    }

    public static IOptions<WindowManagementOptions> WindowManagementOptions(WindowManagementOptions? options = null)
    {
        options ??= new();
        var mock = new Mock<IOptions<WindowManagementOptions>>();
        mock.Setup(x => x.Value).Returns(options);
        return mock.Object;
    }
}
