using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05StreamingSource
{
    [Fact]
    public void Prefetch_LoadsRealSourceOnceAndAdoptsOnCaller()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        a.SetHeight(1800, 1800, 1234);
        var source = new NasademElevationSource(new NasademTileCatalog([a.Path, b.Path]), 1);
        Assert.False(source.PrefetchRaw(24, 120));
        Assert.True(source.RawPrefetchPending);
        Assert.Equal(0L, source.Residency.LoadCount);
        Assert.False(source.PrefetchRaw(24, 121));
        Assert.True(SpinWait.SpinUntil(() =>
        {
            source.PumpRawPrefetch();
            return !source.RawPrefetchPending;
        }, TimeSpan.FromSeconds(20)));
        Assert.Equal(1L, source.BackgroundLoadCount);
        Assert.True(source.PrefetchRaw(24, 120));
        Assert.Equal(1234d, source.Sample(24.5, 120.5));
        Assert.Equal(1L, source.Residency.LoadCount);
        Assert.False(source.PrefetchRaw(24, 121));
        Assert.NotNull(source.ReadSummary(24, 121));
        Assert.Equal(2L, source.BackgroundLoadCount);
        Assert.Equal(2L, source.Residency.LoadCount);
        Assert.Equal(1L, source.Residency.EvictedCount);
        Assert.False(source.RawPrefetchPending);
    }

    [Fact]
    public void PersistentMiddle_IsBoundedAndReloadsExactDerivedSamples()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        using var c = new G02BHgtFixture("N24E122");
        a.SetHeight(1800, 1800, 1234);
        b.SetHeight(1800, 1800, 2345);
        c.SetHeight(1800, 1800, 3456);
        string dir = Path.Combine(AppContext.BaseDirectory, $"XYE-Streaming-{Guid.NewGuid():N}");
        try
        {
            var catalog = new NasademTileCatalog([a.Path, b.Path, c.Path]);
            var source = new NasademElevationSource(catalog, 1, true, dir, 2);
            Assert.Equal(2, source.OverviewMiddleCount);
            Assert.Equal(3 * 7442L + 2 * 181202L, source.OverviewBytes);
            var middle = source.CoarseFor(10);
            for (int round = 0; round < 3; round++)
            {
                Assert.Equal(1234d, middle.Sample(24.5, 120.5));
                Assert.Equal(2345d, middle.Sample(24.5, 121.5));
                Assert.Equal(3456d, middle.Sample(24.5, 122.5));
                Assert.Equal(2, source.OverviewMiddleCount);
            }
            Assert.Equal(0L, source.Residency.LoadCount);
            var warm = new NasademElevationSource(catalog, 1, true, dir, 2);
            Assert.Equal(3, warm.OverviewCacheHits);
            Assert.Equal(1234d, warm.CoarseFor(10).Sample(24.5, 120.5));
            Assert.Equal(2, warm.OverviewMiddleCount);
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }
}