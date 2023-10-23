using cswm.Arrangement.Partitioning;
using cswm.WinApi;
using Xunit;

namespace cswm.Tests.Arrangement.Partitioning;

public class BspTreeTests
{
	public Rect MonitorSize => new(0, 0, 1920, 1080);

	[Fact]
	public void IsLeaf_ReturnsTrue_IfPartitionNull()
	{
		var node = new BspTree();

		Assert.True(node.IsLeaf);
	}
}
