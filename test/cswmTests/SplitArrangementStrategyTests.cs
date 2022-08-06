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
        var windows = GetWindows(2).ToArray();
        var monitor = GetMonitors(1).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layouts = strategy.Arrange(new[] { monitorLayout }).ToArray();
        var positions = layouts.Select(l => l.Position);

        Assert.NotNull(layouts);
        Assert.Equal(2, layouts.Count());
        Assert.Contains(new Rect(0, 0, 960, 1027), positions);
        Assert.Contains(new Rect(960, 0, 1920, 1027), positions);
    }

    [Fact]
    public void Arrange_ThreeWindows_2To1To1Split()
    {
        var strategy = new SplitArrangementStrategy();
        var windows = GetWindows(3).ToArray();
        var monitor = GetMonitors(1).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layouts = strategy.Arrange(new[] { monitorLayout }).ToArray();
        var positions = layouts.Select(l => l.Position);

        Assert.NotNull(layouts);
        Assert.Equal(3, layouts.Count());
        Assert.Contains(new Rect(0, 0, 960, 1027), positions);
        Assert.Contains(new Rect(960, 0, 1920, 514), positions);
        Assert.Contains(new Rect(960, 514, 1920, 1027), positions);
    }

    [Fact]
    public void Arrange_FourWindows_4To2To1To1Split()
    {
        var strategy = new SplitArrangementStrategy();
        var windows = GetWindows(4).ToArray();
        var monitor = GetMonitors(1).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layouts = strategy.Arrange(new[] { monitorLayout }).ToArray();
        var positions = layouts.Select(l => l.Position);

        Assert.NotNull(layouts);
        Assert.Equal(4, layouts.Count());
        Assert.Contains(new Rect(0, 0, 960, 1027), positions);
        Assert.Contains(new Rect(960, 0, 1920, 514), positions);
        Assert.Contains(new Rect(960, 514, 1440, 1027), positions);
        Assert.Contains(new Rect(1440, 514, 1920, 1027), positions);
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