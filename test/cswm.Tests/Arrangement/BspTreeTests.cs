using System;
using cswm.Arrangement;
using cswm.WinApi;
using Xunit;

namespace cswm.Tests.Arrangement;

public class BspTreeTests
{
	public Rect MonitorSize => new(0, 0, 1920, 1080);

	[Fact]
	public void CalcSplits_ThrowsException_IfPartitionNull()
	{
		var tree = new BspTree(MonitorSize);

		var excp = Assert.Throws<NullReferenceException>(() => tree.CalcSplits());

		Assert.Equal("Cannot calculate splits for null partition.", excp.Message);
	}
}
