using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07TileAddress
{
    [Fact]
    public void CandidateRoot_HasExactBoundsAndFourChildren()
    {
        var tile = new GeoUnifiedTile(6, 529, 199);
        Assert.Equal(1083392, tile.West);
        Assert.Equal(407552, tile.South);
        Assert.Equal(2048, tile.Span);
        var child = tile.Child(1, 1);
        Assert.Equal(new GeoUnifiedTile(5, 1059, 399), child);
        Assert.Equal(tile, child.Parent());
        Assert.Equal(tile.West + 1024, child.West);
        Assert.Equal(tile.South + 1024, child.South);
    }
}
