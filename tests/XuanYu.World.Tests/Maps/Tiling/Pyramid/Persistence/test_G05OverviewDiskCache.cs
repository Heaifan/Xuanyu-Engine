using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05OverviewDiskCache
{
    [Fact]
    public void RealZip_WarmImportReusesBothLevelsAndRepairsTruncatedCache()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1800, 1800, 1234);
        string dir = Path.Combine(AppContext.BaseDirectory, $"XYE-Pyramid-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            string zip = Path.Combine(dir, "source.zip");
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(hgt.Path, "NASADEM_HGT_n24e121.hgt");
            string cache = Path.Combine(dir, "cache");
            var cold = Open(zip, cache);
            Assert.Equal(0, cold.OverviewCacheHits);
            var warm = Open(zip, cache);
            Assert.Equal(1, warm.OverviewCacheHits);
            Assert.Equal(188644L, warm.OverviewBytes);
            Assert.Equal(1234d, warm.CoarseFor(9).Sample(24.5, 121.5));
            Assert.Equal(1234d, warm.CoarseFor(10).Sample(24.5, 121.5));
            Assert.Equal(0L, warm.Residency.LoadCount);
            string far = Assert.Single(Directory.EnumerateFiles(cache,
                "*.s60.xyheight", SearchOption.AllDirectories));
            using (var stream = File.OpenWrite(far)) stream.SetLength(20);
            var repaired = Open(zip, cache);
            Assert.Equal(0, repaired.OverviewCacheHits);
            Assert.Equal(1234d, repaired.CoarseFor(9).Sample(24.5, 121.5));
            Assert.Equal(1, Open(zip, cache).OverviewCacheHits);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void ChangedSourceAndDifferentPath_DoNotReuseOldHeights()
    {
        using var a = new G02BHgtFixture("N24E121");
        using var b = new G02BHgtFixture("N24E121");
        string dir = Path.Combine(AppContext.BaseDirectory, $"XYE-Pyramid-{Guid.NewGuid():N}");
        try
        {
            a.SetHeight(1800, 1800, 100);
            b.SetHeight(1800, 1800, 300);
            Assert.Equal(0, Open(a.Path, dir).OverviewCacheHits);
            var other = Open(b.Path, dir);
            Assert.Equal(0, other.OverviewCacheHits);
            Assert.Equal(300d, other.CoarseFor(9).Sample(24.5, 121.5));
            DateTime before = File.GetLastWriteTimeUtc(a.Path);
            a.SetHeight(1800, 1800, 200);
            File.SetLastWriteTimeUtc(a.Path, before.AddSeconds(5));
            var changed = Open(a.Path, dir);
            Assert.Equal(0, changed.OverviewCacheHits);
            Assert.Equal(200d, changed.CoarseFor(9).Sample(24.5, 121.5));
            Assert.Equal(1, Open(a.Path, dir).OverviewCacheHits);
            Assert.Equal(2, Directory.GetFiles(dir, "source.stamp", SearchOption.AllDirectories).Length);
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    private static NasademElevationSource Open(string path, string cache) =>
        new(new NasademTileCatalog([path]), cacheCapacity: 1,
            useOverview: true, overviewCacheDirectory: cache);
}