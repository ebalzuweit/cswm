using cswm.Tests;
using cswm.WinApi;
using cswm.WindowManagement.Arrangement;
using System.Linq;
using Xunit;

public class SplitArrangementStrategyTests
{
    private SplitArrangementStrategy Strategy => new(Mocks.Logger<SplitArrangementStrategy>(), Mocks.WindowManagementOptions());
    private Rect MonitorSize => new(0, 0, 1920, 1080);

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
        var positions = layout.Windows.Select(x => x.Position);

        Assert.NotNull(layout);
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

        var layouts = Strategy.Arrange(monitorLayout);
        var positions = layouts.Windows.Select(x => x.Position);

        Assert.NotNull(layouts);
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
        var positions = layout.Windows.Select(x => x.Position);

        Assert.NotNull(layout);
        Assert.Equal(4, positions.Count());
        Assert.Contains(new Rect(0, 0, 960, 1080), positions);
        Assert.Contains(new Rect(960, 0, 1920, 540), positions);
        Assert.Contains(new Rect(960, 540, 1440, 1080), positions);
        Assert.Contains(new Rect(1440, 540, 1920, 1080), positions);
    }
}