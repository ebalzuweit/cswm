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
        var layouts = windows.Select(w => new WindowLayout(w, new Rect(), monitor));

        var arrangement = strategy.Arrange(layouts).ToArray();

        Assert.NotNull(arrangement);
        Assert.Equal(2, arrangement.Count());
        Assert.All(arrangement, layout =>
        {
            const string windowInsideMonitorMessage = "Window should be positioned inside monitor.";
            Assert.True(layout.Position.Left >= monitor.Position.Left, windowInsideMonitorMessage);
            Assert.True(layout.Position.Top >= monitor.Position.Top, windowInsideMonitorMessage);
            Assert.True(layout.Position.Right <= monitor.Position.Right, windowInsideMonitorMessage);
            Assert.True(layout.Position.Bottom <= monitor.Position.Bottom, windowInsideMonitorMessage);

            Assert.True(layout.Position.Right > layout.Position.Left, "Window must have positive width.");
            Assert.True(layout.Position.Bottom > layout.Position.Top, "Window must have positive height.");
        });
        Assert.Equal(arrangement[0].Position.Area, arrangement[1].Position.Area);
    }

    [Fact]
    public void Arrange_TwoWindows_PreserveSpatialRelation()
    {
        var strategy = new SplitArrangementStrategy();
        var windows = GetWindows(2).ToArray();
        var monitor = GetMonitors(1).First();
        var layouts = new[] {
            new WindowLayout(windows[0], new Rect(0, 0, 100, 100), monitor),
            new WindowLayout(windows[1], new Rect(100, 0, 200, 100), monitor),
        };

        var arrangement = strategy.Arrange(layouts).ToArray();

        Assert.NotNull(arrangement);
        Assert.True(arrangement[0].Position.Left < arrangement[1].Position.Left, "Windows should preserve initial spatial relationship.");
        Assert.False(arrangement[0].Window.Position.Overlaps(arrangement[1].Window.Position), "Windows should not overlap.");
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