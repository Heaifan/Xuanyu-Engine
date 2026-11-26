using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07LodRetry
{
    [G07DataFact]
    public async Task TransientReadFailureCanRecoverWithoutReplacingCommittedParent()
    {
        string folder = Path.Combine(Path.GetTempPath(), "XYE-G07-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        string original = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        string path = Path.Combine(folder, "NASADEM_HGT_n23e120.zip");
        try
        {
            File.Copy(original, path);
            var catalog = new NasademTileCatalog([path]);
            var loader = new GeoUnifiedTileLoader(catalog, 100_000_000);
            var source = new GeoUnifiedSamples(catalog, "v1", loader);
            var cache = new GeoUnifiedPageCache(loader, source);
            var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
            var root = new GeoUnifiedTile(1, 16930, 6368);
            var anchor = new EnuFrame(new GeoCoordinate(23.2177777778, 120.9866666667, 0));
            using var session = await GeoUnifiedLodSession.CreateAsync(root, pages, cache, loader, source, anchor);
            EnuPosition eye;
            using (var page = session.AcquirePage(root, 0)!)
            {
                var bounds = GeoUnifiedBounds.Measure(root, source, anchor)!.Value;
                eye = new((bounds.Min.East + bounds.Max.East) / 2,
                    (bounds.Min.North + bounds.Max.North) / 2, (bounds.Min.Up + bounds.Max.Up) / 2);
            }
            Assert.True(loader.Release(23, 120));
            File.Delete(path);
            bool observed = false;
            for (int i = 0; i < 500; i++)
            {
                var current = session.Step(eye, 1080, Math.PI / 3, (_, _) => true);
                Assert.Contains(root, current.Coverage.Leaves);
                if (session.Diagnostic is not null) { observed = true; break; }
                await Task.Delay(10);
            }
            Assert.True(observed);
            File.Copy(original, path);
            GeoUnifiedSelection recovered = default;
            for (int i = 0; i < 1000; i++)
            {
                recovered = session.Step(eye, 1080, Math.PI / 3, (_, _) => true);
                if (recovered.Coverage.Leaves.Count == 4) break;
                await Task.Delay(10);
            }
            Assert.Equal(4, recovered.Coverage.Leaves.Count);
        }
        finally { Directory.Delete(folder, true); }
    }
}
