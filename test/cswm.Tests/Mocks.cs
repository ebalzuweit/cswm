﻿using cswm.Arrangement;
using cswm.Options;
using cswm.WinApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Windows.Win32.Foundation;

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
            monitors[i] = new Monitor
            {
                WorkArea = size
            };
        }
        return monitors;
    }

    public static WindowLayout[] GetWindowLayouts(int count = 1)
    {
        var windows = new WindowLayout[count];
        for (int i = 0; i < count; i++)
        {
            windows[i] = Mocks.WindowLayout(new());
        }
        return windows;
    }

    public static Window Window(Rect position, string tag = "", IntPtr? hWnd = null)
    {
        if (hWnd.HasValue == false) hWnd = new IntPtr(Random.Shared.Next());
        var hwnd = new HWND(hWnd.Value);
        return new Window
        {
            hWnd = hwnd,
            ClassName = tag,
            Position = position,
            ClientPosition = position
        };
    }

    public static WindowLayout WindowLayout(Rect position, string tag = "", IntPtr? hWnd = null)
        => new WindowLayout(Mocks.Window(position, tag, hWnd), position);

    public static IOptions<WindowManagementOptions> WindowManagementOptions(WindowManagementOptions? options = null)
    {
        options ??= new();
        var mock = new Mock<IOptions<WindowManagementOptions>>();
        mock.SetupGet(x => x.Value).Returns(options);
        return mock.Object;
    }
}
