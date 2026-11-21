using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_NasademResidentTile
{
    [Fact]
    public void ResidentTile_UsesDecodedRawSamplesWithoutLoadingOnLookup()
    {
        using var fixture = new G02BHgtFixture("N24E121");
        fixture.SetHeight(0, 3600, -20);
        fixture.SetHeight(1, 3600, short.MinValue);
        fixture.SetHeight(0, 0, 300);
        var catalog = new NasademTileCatalog([fixture.Path]);
        var source = new NasademElevationSource(catalog);

        Assert.Null(source.TryGetResidentTile(24, 121));
        Assert.Equal(0d, source.Sample(24.5d, 121.5d));

        var tile = Assert.IsType<NasademTile>(source.TryGetResidentTile(24, 121));
        ReadOnlySpan<short> samples = tile.SamplesNorthToSouth.Span;
        Assert.Equal(NasademTile.SampleCount, samples.Length);
        Assert.Equal((short)-20, samples[0]);
        Assert.Equal(short.MinValue, samples[1]);
        Assert.Equal((short)300,
            samples[(NasademTile.Side - 1) * NasademTile.Side]);
        Assert.Null(source.TryGetResidentTile(24, 122));
    }
}