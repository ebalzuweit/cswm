using cswm.Arrangement;
using cswm.WinApi;
using System.Linq;
using Xunit;

namespace cswm.Tests.Arrangement;

public class SplitArrangementStrategyTests
{
    private SplitArrangementStrategy Strategy => new(Mocks.WindowManagementOptions());
    private SplitArrangementStrategy StrategyWithPadding => new(Mocks.WindowManagementOptions(new()
    {
        MonitorPadding = 100
    }));
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

    [Fact]
    public void Arrange_HasMonitorPadding_WhenNonzero()
    {
        var windows = Mocks.GetWindowLayouts(1);
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layout = StrategyWithPadding.Arrange(monitorLayout);

        Assert.NotNull(layout);

        var positions = layout.Windows.Select(x => x.Position);

        Assert.Contains(new Rect(100, 100, 1820, 980), positions);
    }

    #endregion

    #region Window Move Tests

    [Fact]
    public void ArrangeOnWindowMove_PrefersMovedWindow_IfWindowsOverlapping()
    {
        var aPosition = new Rect(960, 0, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var b = Mocks.WindowLayout(aPosition, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(monitorLayout, a.Window, new(1440, 540));

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

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(monitorLayout, a.Window, new(480, 540));

        Assert.NotNull(layout);
        Assert.Equal(aPosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(bPosition, layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
        Assert.Equal(cPosition, layout.Windows.Where(x => x.Window.ClassName == "c").First().Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_SwapsExistingWindowPositions_IfCursorInOtherWindow()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var aMovePosition = new Rect(1000, 100, 1960, 1180);
        var bPosition = new Rect(960, 0, 1920, 540);
        var cPosition = new Rect(960, 540, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var aMove = Mocks.WindowLayout(aMovePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(bPosition, "b");
        var c = Mocks.WindowLayout(cPosition, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var movedLayout = new MonitorLayout(monitor, new[] { aMove, b, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(movedLayout, a.Window, new(1440, 270));

        Assert.NotNull(layout);
        Assert.Equal(bPosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(aPosition, layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
        Assert.Equal(cPosition, layout.Windows.Where(x => x.Window.ClassName == "c").First().Position);
    }

    #endregion

    #region Window Resize Tests

    [Fact]
    public void ArrangeOnWindowMove_ResizeOneSide_WithTwoWindows()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var aResizePosition = new Rect(0, 0, 1100, 1080);
        var bPosition = new Rect(960, 0, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var aResize = Mocks.WindowLayout(aResizePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(bPosition, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });
        var resizedLayout = new MonitorLayout(monitor, new[] { aResize, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, aResize.Window, new(1100, 270));

        Assert.NotNull(layout);
        Assert.Equal(aResizePosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(new Rect(1100, 0, 1920, 1080), layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_ResizeCorner_WithTwoWindows()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var aResizePosition = new Rect(0, 0, 1100, 900);
        var bPosition = new Rect(960, 0, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var aResize = Mocks.WindowLayout(aResizePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(bPosition, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });
        var resizedLayout = new MonitorLayout(monitor, new[] { aResize, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, aResize.Window, new(1100, 270));

        Assert.NotNull(layout);
        Assert.Equal(new Rect(0, 0, 1100, 1080), layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(new Rect(1100, 0, 1920, 1080), layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_ResizeOneSide_WithThreeWindows()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var aResizePosition = new Rect(0, 0, 1100, 1080);
        var bPosition = new Rect(960, 0, 1920, 540);
        var cPosition = new Rect(960, 540, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var aResize = Mocks.WindowLayout(aResizePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(bPosition, "b");
        var c = Mocks.WindowLayout(cPosition, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var resizedLayout = new MonitorLayout(monitor, new[] { aResize, b, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, aResize.Window, new(1100, 270));

        Assert.NotNull(layout);
        Assert.Equal(aResizePosition, layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(new Rect(1100, 0, 1920, 540), layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
        Assert.Equal(new Rect(1100, 540, 1920, 1080), layout.Windows.Where(x => x.Window.ClassName == "c").First().Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_ResizeCorner_WithThreeWindows()
    {
        var aPosition = new Rect(0, 0, 960, 1080);
        var bPosition = new Rect(960, 0, 1920, 540);
        var bResizePosition = new Rect(1100, 0, 1920, 400);
        var cPosition = new Rect(960, 540, 1920, 1080);

        var a = Mocks.WindowLayout(aPosition, "a");
        var b = Mocks.WindowLayout(bPosition, "b");
        var bResize = Mocks.WindowLayout(bResizePosition, "b", b.Window.hWnd);
        var c = Mocks.WindowLayout(cPosition, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var resizedLayout = new MonitorLayout(monitor, new[] { a, bResize, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, bResize.Window, new(1100, 400));

        Assert.NotNull(layout);
        Assert.Equal(new Rect(0, 0, 1100, 1080), layout.Windows.Where(x => x.Window.ClassName == "a").First().Position);
        Assert.Equal(bResizePosition, layout.Windows.Where(x => x.Window.ClassName == "b").First().Position);
        Assert.Equal(new Rect(1100, 400, 1920, 1080), layout.Windows.Where(x => x.Window.ClassName == "c").First().Position);
    }

    #endregion
}