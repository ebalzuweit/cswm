using cswm.Core.Layout;

namespace cswm.Core.Test.Layout;

public class RectTest
{
	[Theory]
	[InlineData(0, 1000, 1000)]
	[InlineData(-500, 500, 1000)]
	public void Width_ReturnsWidth(int left, int right, int expectedWidth)
	{
		var rect = new Rect(left, 0, right, 0);

		var width = rect.Width;

		Assert.Equal(expectedWidth, width);
	}

	[Theory]
	[InlineData(0, 1000, 1000)]
	[InlineData(-500, 500, 1000)]
	public void Height_ReturnsHeight(int top, int bottom, int expectedHeight)
	{
		var rect = new Rect(0, top, 0, bottom);

		var height = rect.Height;

		Assert.Equal(expectedHeight, height);
	}
}
