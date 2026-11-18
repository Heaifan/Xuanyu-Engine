using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04NoData
{
    [Fact]
    public void MissingSample_DoesNotBecomeZeroHeightTriangle()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1, 1, short.MinValue);
        NasademTile source = hgt.Load();
        var block = new NasademSampleBlock(source, 0, 0);
        var anchor = new EnuFrame(new GeoCoordinate(24.5d, 121.5d, 0d));

        Assert.Null(block.HeightAt(1, 1));
        TerrainMeshGeometry mesh = NasademBlockMeshBuilder.Build(block, anchor);
        Assert.Equal(300 * 300 * 2 - 6, mesh.TriangleCount);
        Assert.DoesNotContain(301 + 1, mesh.Indices);
        Assert.Null(source.HeightAt(1, 1));
    }
}