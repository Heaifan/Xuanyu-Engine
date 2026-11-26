using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Version
{
    [G07DataFact]
    public void ReplacingAnAlreadyPublishedTileRequiresNewSourceVersion()
    {
        string west = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        var catalog = new NasademTileCatalog([west]);
        var original = NasademHgtFile.Load(west);
        var next = NasademHgtFile.Load(west);
        var source = new GeoUnifiedSamples(catalog, "v1");
        source.Add(original);
        source.Add(original);
        Assert.Throws<InvalidOperationException>(() => source.Add(next));
        var newer = new GeoUnifiedSamples(catalog, "v2");
        newer.Add(next);
        Assert.Equal("v1", source.Sample((180 + 120) * 3600 + 1,
            (90 + 23) * 3600 + 1).Version);
        Assert.Equal("v2", newer.Sample((180 + 120) * 3600 + 1,
            (90 + 23) * 3600 + 1).Version);
    }
}
