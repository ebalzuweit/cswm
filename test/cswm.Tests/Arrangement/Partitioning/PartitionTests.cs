using cswm.Arrangement.Partitioning;
using Xunit;

namespace cswm.Tests.Arrangement.Partitioning;

public sealed class PartitionTests
{
	[Fact]
	public void SetPosition_WithNegativeValue_SetsToZero()
	{
		var partition = new Partition(true, -1);

		Assert.Equal(0, partition.Position);
	}
}