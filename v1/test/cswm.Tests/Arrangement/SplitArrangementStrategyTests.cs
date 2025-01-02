using cswm.Arrangement;
using cswm.WinApi;
using System.Drawing;
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

    private readonly Rect MonitorSize = new(0, 0, 1920, 1080);
    private readonly Rect HalfLeft = new(0, 0, 960, 1080);
    private readonly Rect HalfRight = new(960, 0, 1920, 1080);
    private readonly Rect QuarterTopRight = new(960, 0, 1920, 540);
    private readonly Rect QuarterBottomRight = new(960, 540, 1920, 1080);

    #region Window Arrange Tests

    [Fact]
    public void Arrange_OneWindow_FillsMonitor()
    {
        var windows = Mocks.GetWindowLayouts(1);
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, windows);

        var layout = Strategy.Arrange(monitorLayout);

        Assert.NotNull(layout);
        Assert.Equal(MonitorSize, layout.Windows.First().Position);
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
        Assert.Contains(HalfLeft, positions);
        Assert.Contains(HalfRight, positions);
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
        Assert.Contains(HalfLeft, positions);
        Assert.Contains(QuarterTopRight, positions);
        Assert.Contains(QuarterBottomRight, positions);
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
        Assert.Contains(HalfLeft, positions);
        Assert.Contains(QuarterTopRight, positions);
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
        var a = Mocks.WindowLayout(HalfRight, "a");
        var b = Mocks.WindowLayout(HalfRight, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(monitorLayout, a.Window, new(1440, 540));

        Assert.NotNull(layout);
        Assert.Equal(HalfRight, layout.GetWindowByClassName("a").Position);
        Assert.Equal(HalfLeft, layout.GetWindowByClassName("b").Position);
    }

    [Fact]
    public void ArrangeOnWindowMove_KeepsExistingWindowPositions_IfCursorInMovedWindow()
    {
        Point pos = new(480, 540);

        var a = Mocks.WindowLayout(HalfLeft, "a");
        var b = Mocks.WindowLayout(QuarterTopRight, "b");
        var c = Mocks.WindowLayout(QuarterBottomRight, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(monitorLayout, a.Window, pos);

        Assert.NotNull(layout);
        Assert.Equal(HalfLeft, layout.GetWindowByClassName("a").Position);
        Assert.Equal(QuarterTopRight, layout.GetWindowByClassName("b").Position);
        Assert.Equal(QuarterBottomRight, layout.GetWindowByClassName("c").Position);
    }

    [Fact(Skip = "FIXME")]
    public void ArrangeOnWindowMove_SwapsExistingWindowPositions_IfCursorInOtherWindow()
    {
        Point pos = new(1440, 270);
        var movePos = HalfLeft.MoveTo(pos);

        var a = Mocks.WindowLayout(HalfLeft, "a");
        var aMove = Mocks.WindowLayout(movePos, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(QuarterTopRight, "b");
        var c = Mocks.WindowLayout(QuarterBottomRight, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var movedLayout = new MonitorLayout(monitor, new[] { aMove, b, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(movedLayout, a.Window, new(1440, 270));

        Assert.NotNull(layout);
        Assert.Equal(QuarterTopRight, layout.GetWindowByClassName("a").Position);
        Assert.Equal(HalfLeft, layout.GetWindowByClassName("b").Position);
        Assert.Equal(QuarterBottomRight, layout.GetWindowByClassName("c").Position);
    }

    #endregion

    #region Window Resize Tests

    [Fact(Skip = "FIXME")]
    public void ArrangeOnWindowMove_ResizeOneSide_WithTwoWindows()
    {
        var aResizePosition = new Rect(0, 0, 1100, 1080);

        var a = Mocks.WindowLayout(HalfLeft, "a");
        var aResize = Mocks.WindowLayout(aResizePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(HalfRight, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });
        var resizedLayout = new MonitorLayout(monitor, new[] { aResize, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, aResize.Window, new(1100, 270));

        Assert.NotNull(layout);
        Assert.Equal(aResizePosition, layout.GetWindowByClassName("a").Position);
        Assert.Equal(new Rect(1100, 0, 1920, 1080), layout.GetWindowByClassName("b").Position);
    }

    [Fact(Skip = "FIXME")]
    public void ArrangeOnWindowMove_ResizeCorner_WithTwoWindows()
    {
        var aResizePosition = new Rect(0, 0, 1100, 900);

        var a = Mocks.WindowLayout(HalfLeft, "a");
        var aResize = Mocks.WindowLayout(aResizePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(HalfRight, "b");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });
        var resizedLayout = new MonitorLayout(monitor, new[] { aResize, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, aResize.Window, new(1100, 270));

        Assert.NotNull(layout);
        Assert.Equal(new Rect(0, 0, 1100, 1080), layout.GetWindowByClassName("a").Position);
        Assert.Equal(new Rect(1100, 0, 1920, 1080), layout.GetWindowByClassName("b").Position);
    }

    [Fact(Skip = "FIXME")]
    public void ArrangeOnWindowMove_ResizeOneSide_WithThreeWindows()
    {
        var aResizePosition = new Rect(0, 0, 1100, 1080);

        var a = Mocks.WindowLayout(HalfLeft, "a");
        var aResize = Mocks.WindowLayout(aResizePosition, "a", a.Window.hWnd);
        var b = Mocks.WindowLayout(QuarterTopRight, "b");
        var c = Mocks.WindowLayout(QuarterBottomRight, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var resizedLayout = new MonitorLayout(monitor, new[] { aResize, b, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, aResize.Window, new(1100, 270));

        Assert.NotNull(layout);
        Assert.Equal(aResizePosition, layout.GetWindowByClassName("a").Position);
        Assert.Equal(new Rect(1100, 0, 1920, 540), layout.GetWindowByClassName("b").Position);
        Assert.Equal(new Rect(1100, 540, 1920, 1080), layout.GetWindowByClassName("c").Position);
    }

    [Fact(Skip = "FIXME")]
    public void ArrangeOnWindowMove_ResizeCorner_WithThreeWindows()
    {
        var bResizePosition = new Rect(1100, 0, 1920, 400);

        var a = Mocks.WindowLayout(HalfLeft, "a");
        var b = Mocks.WindowLayout(QuarterTopRight, "b");
        var bResize = Mocks.WindowLayout(bResizePosition, "b", b.Window.hWnd);
        var c = Mocks.WindowLayout(QuarterBottomRight, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var resizedLayout = new MonitorLayout(monitor, new[] { a, bResize, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.ArrangeOnWindowMove(resizedLayout, bResize.Window, new(1100, 400));

        Assert.NotNull(layout);
        Assert.Equal(new Rect(0, 0, 1100, 1080), layout.GetWindowByClassName("a").Position);
        Assert.Equal(bResizePosition, layout.GetWindowByClassName("b").Position);
        Assert.Equal(new Rect(1100, 400, 1920, 1080), layout.GetWindowByClassName("c").Position);
    }

    #endregion

    #region Window Open Tests

    [Fact]
    public void Arrange_PreservesExistingPositions_OnNewWindow()
    {
        var a = Mocks.WindowLayout(HalfRight, "a");
        var b = Mocks.WindowLayout(HalfLeft, "b");
        var c = Mocks.WindowLayout(QuarterBottomRight, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b });
        var updatedLayout = new MonitorLayout(monitor, new[] { a, b, c });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.Arrange(updatedLayout);

        Assert.NotNull(layout);
        Assert.Equal(QuarterTopRight, layout.GetWindowByClassName("a").Position);
        Assert.Equal(HalfLeft, layout.GetWindowByClassName("b").Position);
        Assert.Equal(QuarterBottomRight, layout.GetWindowByClassName("c").Position);
    }

    #endregion

    #region Window Close Tests

    [Fact]
    public void Arrange_PreservesExistingPositions_OnWindowClose()
    {
        var a = Mocks.WindowLayout(HalfLeft, "a");
        var b = Mocks.WindowLayout(QuarterTopRight, "b");
        var c = Mocks.WindowLayout(QuarterBottomRight, "c");
        var monitor = Mocks.GetMonitors(MonitorSize).First();
        var monitorLayout = new MonitorLayout(monitor, new[] { a, b, c });
        var updatedLayout = new MonitorLayout(monitor, new[] { a, b });

        var strategy = Strategy;
        _ = strategy.Arrange(monitorLayout);
        var layout = strategy.Arrange(updatedLayout);

        Assert.NotNull(layout);
        Assert.Equal(HalfLeft, layout.GetWindowByClassName("a").Position);
        Assert.Equal(HalfRight, layout.GetWindowByClassName("b").Position);
    }

    #endregion
}