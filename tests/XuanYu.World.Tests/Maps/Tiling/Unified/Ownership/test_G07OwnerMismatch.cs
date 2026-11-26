using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07OwnerMismatch
{
    [Fact]
    public void UnrelatedLoadersCannotShareOnePageBudgetOrSource()
    {
        var catalog = new NasademTileCatalog([]);
        var otherCatalog = new NasademTileCatalog([]);
        var first = new GeoUnifiedTileLoader(catalog, 100_000_000);
        var second = new GeoUnifiedTileLoader(catalog, 100_000_000);
        Assert.Throws<ArgumentException>(() => new GeoUnifiedSamples(otherCatalog, "v1", first));
        var source = new GeoUnifiedSamples(catalog, "v1", first);
        Assert.Throws<ArgumentException>(() => new GeoUnifiedPageCache(second, source));
        var cache = new GeoUnifiedPageCache(first, source);
        Assert.Throws<ArgumentException>(() => new GeoUnifiedPageLoader(catalog, second, cache));
        Assert.Throws<ArgumentException>(() => new GeoUnifiedPageLoader(otherCatalog, first, cache));
    }
}
