using System;
using cswm.Core.Layout;
using cswm.Core.Layout.Engines;

namespace cswm.Core.Test.Layout.Engines;

public class BspTiliingLayoutEngineTest
{
	private Bounds Bounds_1920x1080 = new(0, 0, 1920, 1080);
	private Func<WindowInfo> GetWindowInfo = () => new WindowInfo(0, string.Empty, string.Empty, new(0, 0, 1, 1), false, false);

	[Fact]
	public void CalculateLayout_ReturnsEmptyLayout_WithNoWindows()
	{
		var engine = new BspTilingLayoutEngine();

		var result = engine.CalculateLayout(Bounds_1920x1080, []);

		Assert.NotNull(result);
		Assert.Empty(result.WindowLayouts);
	}

	[Fact]
	public void CalculateLayout_ReturnsEntireBounds_WithSingleWindow()
	{
		var engine = new BspTilingLayoutEngine();
		var bounds = Bounds_1920x1080;
		var windows = new WindowInfo[]
		{
			GetWindowInfo()
		};

		var result = engine.CalculateLayout(bounds, windows);

		Assert.NotNull(result);
		Assert.Single(result.WindowLayouts);
		Assert.Equal(bounds, result.WindowLayouts.First().Area);
	}

	[Fact]
	public void CalculateLayout_ReturnsHorizontalSplit_WithTwoWindows()
	{
		var engine = new BspTilingLayoutEngine();
		var bounds = Bounds_1920x1080;
		var windows = new WindowInfo[]
		{
			GetWindowInfo(),
			GetWindowInfo()
		};

		var result = engine.CalculateLayout(bounds, windows);

		Assert.NotNull(result);
		Assert.Equal(2, result.WindowLayouts.Count);

		var areas = result.WindowLayouts.Select(x => x.Area).ToArray();
		var split = bounds.Left + (bounds.Width / 2);
		Assert.Contains(new Bounds(bounds.Left, bounds.Top, split, bounds.Bottom), areas);
		Assert.Contains(new Bounds(split + 1, bounds.Top, bounds.Right, bounds.Bottom), areas);
	}

	[Fact]
	public void CalculateLayout_ReturnsVerticalSplit_WithTwoWindows()
	{
		var engine = new BspTilingLayoutEngine();
		var bounds = new Bounds(0, 0, 1080, 1920);
		var windows = new WindowInfo[]
		{
			GetWindowInfo(),
			GetWindowInfo()
		};

		var result = engine.CalculateLayout(bounds, windows);

		Assert.NotNull(result);
		Assert.Equal(2, result.WindowLayouts.Count);

		var areas = result.WindowLayouts.Select(x => x.Area).ToArray();
		var split = bounds.Top + (bounds.Height / 2);
		Assert.Contains(new Bounds(bounds.Left, bounds.Top, bounds.Right, split), areas);
		Assert.Contains(new Bounds(bounds.Left, split + 1, bounds.Right, bounds.Bottom), areas);
	}
}
