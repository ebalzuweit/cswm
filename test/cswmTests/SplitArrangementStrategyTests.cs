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
        var monitor = GetMonitors(1).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layouts = strategy.Arrange(new[] { monitorLayout }).ToArray();

        Assert.NotNull(layouts);
        Assert.Equal(2, layouts.Count());
        Assert.All(layouts, layout =>
        {
            const string windowInsideMonitorMessage = "Window should be positioned inside monitor.";
            Assert.True(layout.Position.Left >= monitor.WorkArea.Left, windowInsideMonitorMessage);
            Assert.True(layout.Position.Top >= monitor.WorkArea.Top, windowInsideMonitorMessage);
            Assert.True(layout.Position.Right <= monitor.WorkArea.Right, windowInsideMonitorMessage);
            Assert.True(layout.Position.Bottom <= monitor.WorkArea.Bottom, windowInsideMonitorMessage);

            Assert.True(layout.Position.Right > layout.Position.Left, "Window must have positive width.");
            Assert.True(layout.Position.Bottom > layout.Position.Top, "Window must have positive height.");
        });
        Assert.Equal(layouts[0].Position.Area, layouts[1].Position.Area);
    }

    [Fact]
    public void Arrange_TwoWindows_PreserveSpatialRelation()
    {
        var strategy = new SplitArrangementStrategy();
        var windows = GetWindows(2).ToArray();
        var monitor = GetMonitors(1).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layouts = strategy.Arrange(new[] { monitorLayout }).ToArray();

        Assert.NotNull(layouts);
        Assert.NotEmpty(layouts);
        Assert.True(layouts[0].Position.Left < layouts[1].Position.Left, "Windows should preserve initial spatial relationship.");
        Assert.False(layouts[0].Window.Position.Overlaps(layouts[1].Window.Position), "Windows should not overlap.");
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