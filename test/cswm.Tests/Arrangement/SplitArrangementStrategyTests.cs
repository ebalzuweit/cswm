using cswm.Arrangement;
using cswm.WinApi;
using System.Linq;
using Xunit;

namespace cswm.Tests.Arrangement;

public class SplitArrangementStrategyTests
{
    private SplitArrangementStrategy Strategy => new(Mocks.WindowManagementOptions());
    private Rect MonitorSize => new(0, 0, 1920, 1080);

    #region Window Arrange Tests

    [Fact]
    public void Arrange_OneWindow_FillsMonitor()
    {
        var windows = Mocks.GetWindowLayouts(1);
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layout = Strategy.Arrange(monitorLayout);

        Assert.NotNull(layout);
        Assert.Equal(new Rect(0, 0, 1920, 1080), layout.Windows.First().Position);
    }

    [Fact]
    public void Arrange_TwoWindows_1To1Split()
    {
        var windows = Mocks.GetWindowLayouts(2);
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layout = Strategy.Arrange(monitorLayout);

        Assert.NotNull(layout);

        var positions = layout.Windows.Select(x => x.Position);

        Assert.Equal(2, positions.Count());
        Assert.Contains(new Rect(0, 0, 960, 1080), positions);
        Assert.Contains(new Rect(960, 0, 1920, 1080), positions);
    }

    [Fact]
    public void Arrange_ThreeWindows_2To1To1Split()
    {
        var windows = Mocks.GetWindowLayouts(3);
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layout = Strategy.Arrange(monitorLayout);

        Assert.NotNull(layout);

        var positions = layout.Windows.Select(x => x.Position);

        Assert.Equal(3, positions.Count());
        Assert.Contains(new Rect(0, 0, 960, 1080), positions);
        Assert.Contains(new Rect(960, 0, 1920, 540), positions);
        Assert.Contains(new Rect(960, 540, 1920, 1080), positions);
    }

    [Fact]
    public void Arrange_FourWindows_4To2To1To1Split()
    {
        var windows = Mocks.GetWindowLayouts(4);
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layout = Strategy.Arrange(monitorLayout);

        Assert.NotNull(layout);

        var positions = layout.Windows.Select(x => x.Position);

        Assert.Equal(4, positions.Count());
        Assert.Contains(new Rect(0, 0, 960, 1080), positions);
        Assert.Contains(new Rect(960, 0, 1920, 540), positions);
        Assert.Contains(new Rect(960, 540, 1440, 1080), positions);
        Assert.Contains(new Rect(1440, 540, 1920, 1080), positions);
    }

    #endregion

    #region On Window Move Tests

    [Fact]
    public void ArrangeOnWindowMove_PrefersMovedWindow_IfWindowsOverlapping()
    {
        var aPosition = new Rect(960, 0, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var b = Mocks.WindowLayout(aPosition, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });

        var layout = Strategy.ArrangeOnWindowMove(monitorLayout, a.Window, new(1440, 540));

        Assert.NotNull(layout);
        Assert.Equal(aPosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(new(0, 0, 960, 1080), layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_KeepsExistingWindowPositions_IfCursorInMovedWindow()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var bPosition = new Rect(960, 0, 1920, 540);
        var cPosition = new Rect(960, 540, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var b = Mocks.WindowLayout(bPosition, "b");
        var c = Mocks.WindowLayout(cPosition, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });

        var layout = Strategy.ArrangeOnWindowMove(monitorLayout, a.Window, new(480, 540));

        Assert.NotNull(layout);
        Assert.Equal(aPosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(bPosition, layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
        Assert.Equal(cPosition, layout.Windows.Where(x => x.Window.ClassName == "c").First().Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_SwapsExistingWindowPositions_IfCursorInOtherWindow()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var bPosition = new Rect(960, 0, 1920, 540);
        var cPosition = new Rect(960, 540, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var b = Mocks.WindowLayout(bPosition, "b");
        var c = Mocks.WindowLayout(cPosition, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });

        var layout = Strategy.ArrangeOnWindowMove(monitorLayout, a.Window, new(1440, 270));

        Assert.NotNull(layout);
        Assert.Equal(bPosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(aPosition, layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
        Assert.Equal(cPosition, layout.Windows.Where(x => x.Window.ClassName == "c").First().Position);
    }

    #endregion
}