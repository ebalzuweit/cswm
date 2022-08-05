using System;
using System.Linq;
using cswm.WinApi;
using cswm.WindowManagement;
using cswm.WindowManagement.Arrangement;
using Moq;
using Xunit;

public class SplitArrangementStrategyTests
{
    [Fact]
    public void Arrange_TwoWindows_1To1Split()
    {
        var strategy = new SplitArrangementStrategy();
        var windows = GetWindows(2);
        var monitors = GetMonitors(1);

        var arrangement = strategy.Arrange(windows, monitors);

        Assert.NotNull(arrangement);
        Assert.Equal(2, arrangement._windows.Count());
        Assert.Equal(2, arrangement._positions.Count());
        Assert.All(arrangement._windows, window =>
        {
            Assert.True(window.Position.Left >= 0);
            Assert.True(window.Position.Top >= 0);
        });
        Assert.True(false);
    }

    private Window[] GetWindows(int n)
    {
        var windows = new Window[n];
        for (int i = 0; i < n; i++)
        {
            var mock = new Mock<Window>(IntPtr.Zero);
            windows[i] = mock.Object;
        }
        return windows;
    }

    private Monitor[] GetMonitors(int n)
    {
        var monitors = new Monitor[n];
        for (int i = 0; i < n; i++)
        {
            var mock = new Mock<Monitor>(IntPtr.Zero);
            mock.Setup(monitor => monitor.WorkArea)
                .Returns(new Rect(0, 0, 1920, 1027));
            monitors[i] = mock.Object;
        }
        return monitors;
    }
}