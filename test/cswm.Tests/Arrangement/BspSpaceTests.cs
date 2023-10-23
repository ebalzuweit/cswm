using System;
using System.Linq;
using cswm.Arrangement;
using cswm.WinApi;
using Xunit;

namespace cswm.Tests.Arrangement;

public class BspSpaceTests
{
	public Rect MonitorSize => new(0, 0, 1920, 1080);
	public BspSpace Space => new(MonitorSize, new());

	[Fact]
	public void SetTotalWindowCount_ThrowsException_IfSpaceCountLessThanOne()
		=> Assert.Throws<ArgumentOutOfRangeException>(() => Space.SetTotalWindowCount(0));

	[Fact]
	public void TryResize_ReturnsFalse_IfSpaceCountIsOne()
	{
		var space = Space;

		space.SetTotalWindowCount(1);
		var result = space.TryResize(new(), new());

		Assert.False(result);
	}

	[Fact]
	public void TryResize_ReturnsTrue_IfPartitionMoved()
	{
		var space = Space;

		space.SetTotalWindowCount(2);
		var result = space.TryResize(
			new Rect(0, 0, 960, 1080),
			new Rect(0, 0, 480, 1080)
		);
		var spaces = space.GetSpaces(0).ToList();

		Assert.True(result);
		Assert.Equal(2, spaces.Count);
		Assert.Equal(new Rect(0, 0, 480, 1080), spaces[0]);
		Assert.Equal(new Rect(480, 0, 1920, 1080), spaces[1]);
	}

	[Fact]
	public void GetSpaces_FillsSpace_IfSpaceCountIsOne()
	{
		var space = Space;

		space.SetTotalWindowCount(1);
		var spaces = space.GetSpaces(0).ToList();

		Assert.Single(spaces);
		Assert.Equal(MonitorSize, spaces[0]);
	}

	[Fact]
	public void GetSpaces_SplitsSpace_IfSpaceCountIsTwo()
	{
		var space = Space;

		space.SetTotalWindowCount(2);
		var spaces = space.GetSpaces(0).ToList();

		Assert.Equal(2, spaces.Count);
		Assert.Equal(new Rect(0, 0, 960, 1080), spaces[0]);
		Assert.Equal(new Rect(960, 0, 1920, 1080), spaces[1]);
	}
}
