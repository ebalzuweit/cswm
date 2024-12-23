using System;
using cswm.Core.Layout;

namespace cswm.Core.Test.Layout;

public class BoundsTest
{
	[Fact]
	public void Ctor_ThrowsInvalidOperationException_IfLeftGreaterThanRight()
	{
		var act = () => { _ = new Bounds(1, 0, 0, 0); };

		Assert.Throws<InvalidOperationException>(act);
	}

	[Fact]
	public void Ctor_ThrowsInvalidOperationException_IfTopGreaterThanBottom()
	{
		var act = () => { _ = new Bounds(0, 1, 0, 0); };

		Assert.Throws<InvalidOperationException>(act);
	}

	[Theory]
	[InlineData(0, 1000, 1000)]
	[InlineData(-500, 500, 1000)]
	public void Width_ReturnsWidthOfBounds(int left, int right, int expectedWidth)
	{
		var bounds = new Bounds(left, 0, right, 0);

		var width = bounds.Width;

		Assert.Equal(expectedWidth, width);
	}

	[Theory]
	[InlineData(0, 1000, 1000)]
	[InlineData(-500, 500, 1000)]
	public void Height_ReturnsHeightOfBounds(int top, int bottom, int expectedHeight)
	{
		var bounds = new Bounds(0, top, 0, bottom);

		var height = bounds.Height;

		Assert.Equal(expectedHeight, height);
	}
}
