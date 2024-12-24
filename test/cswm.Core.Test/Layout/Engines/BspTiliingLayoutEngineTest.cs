using System;
using cswm.Core.Layout;
using cswm.Core.Layout.Engines;

namespace cswm.Core.Test.Layout.Engines;

public class BspTiliingLayoutEngineTest
{
	private Rect Rect_1920x1080 = new(0, 0, 1920, 1080);
	private Func<WindowInfo> GetWindowInfo = () => new WindowInfo(0, string.Empty, string.Empty, new(0, 0, 1, 1), false, false);

	[Fact]
	public void CalculateLayout_ReturnsEmptyLayout_WithNoWindows()
	{
		var engine = new BspTilingLayoutEngine();

		var result = engine.CalculateLayout(Rect_1920x1080, []);

		Assert.NotNull(result);
		Assert.Empty(result.WindowLayouts);
	}

	[Fact]
	public void CalculateLayout_ReturnsEntireRect_WithSingleWindow()
	{
		var engine = new BspTilingLayoutEngine();
		var rect = Rect_1920x1080;
		var windows = new WindowInfo[]
		{
			GetWindowInfo()
		};

		var result = engine.CalculateLayout(rect, windows);

		Assert.NotNull(result);
		Assert.Single(result.WindowLayouts);
		Assert.Equal(rect, result.WindowLayouts.First().Area);
	}

	[Fact]
	public void CalculateLayout_ReturnsHorizontalSplit_WithTwoWindows()
	{
		var engine = new BspTilingLayoutEngine();
		var rect = Rect_1920x1080;
		var windows = new WindowInfo[]
		{
			GetWindowInfo(),
			GetWindowInfo()
		};

		var result = engine.CalculateLayout(rect, windows);

		Assert.NotNull(result);
		Assert.Equal(2, result.WindowLayouts.Count);

		var areas = result.WindowLayouts.Select(x => x.Area).ToArray();
		var split = rect.Left + (rect.Width / 2);
		Assert.Contains(new Rect(rect.Left, rect.Top, split, rect.Bottom), areas);
		Assert.Contains(new Rect(split + 1, rect.Top, rect.Right, rect.Bottom), areas);
	}

	[Fact]
	public void CalculateLayout_ReturnsVerticalSplit_WithTwoWindows()
	{
		var engine = new BspTilingLayoutEngine();
		var rect = new Rect(0, 0, 1080, 1920);
		var windows = new WindowInfo[]
		{
			GetWindowInfo(),
			GetWindowInfo()
		};

		var result = engine.CalculateLayout(rect, windows);

		Assert.NotNull(result);
		Assert.Equal(2, result.WindowLayouts.Count);

		var areas = result.WindowLayouts.Select(x => x.Area).ToArray();
		var split = rect.Top + (rect.Height / 2);
		Assert.Contains(new Rect(rect.Left, rect.Top, rect.Right, split), areas);
		Assert.Contains(new Rect(rect.Left, split + 1, rect.Right, rect.Bottom), areas);
	}
}
