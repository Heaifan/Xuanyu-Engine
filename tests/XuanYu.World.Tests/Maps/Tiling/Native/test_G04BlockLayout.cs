using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04BlockLayout
{
    [Fact]
    public void Blocks_CoverAllOriginalSamplesAndShareEdges()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(0, 0, 11);
        hgt.SetHeight(3600, 3600, 22);
        hgt.SetHeight(300, 77, 33);
        hgt.SetHeight(123, 300, 44);
        NasademTile source = hgt.Load();
        var southwest = new NasademSampleBlock(source, 0, 0);
        var east = new NasademSampleBlock(source, 1, 0);
        var north = new NasademSampleBlock(source, 0, 1);
        var northeast = new NasademSampleBlock(source, 11, 11);

        Assert.Equal(12, NasademSampleBlock.BlocksPerEdge);
        Assert.Equal(0, southwest.StartColumn);
        Assert.Equal(3300, northeast.StartColumn);
        Assert.Equal(3300, northeast.StartRowFromSouth);
        Assert.Equal((short)11, southwest.HeightAt(0, 0));
        Assert.Equal((short)22, northeast.HeightAt(300, 300));
        Assert.Equal((short)33, southwest.HeightAt(300, 77));
        Assert.Equal(southwest.HeightAt(300, 77), east.HeightAt(0, 77));
        Assert.Equal((short)44, southwest.HeightAt(123, 300));
        Assert.Equal(southwest.HeightAt(123, 300), north.HeightAt(123, 0));
        Assert.Equal(3600, northeast.StartColumn + 300);
        Assert.Equal(3600, northeast.StartRowFromSouth + 300);
    }

    [Fact]
    public void PublicEntries_RejectOutOfRangeBlocksAndSamples()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        NasademTile source = hgt.Load();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NasademSampleBlock(source, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NasademSampleBlock(source, 12, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NasademSampleBlock(source, 0, 12));
        var block = new NasademSampleBlock(source, 0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => block.HeightAt(301, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => block.HeightAt(0, -1));
    }
}