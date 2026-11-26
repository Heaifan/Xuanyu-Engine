using System.IO.Compression;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07SourceSeam
{
    [G07DataFact]
    public async Task ConflictingSharedEdgeIsCountedAndReportsMaximumDifference()
    {
        string folder = Path.Combine(Path.GetTempPath(), "XYE-G07-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "n23e121.hgt");
        try
        {
            using (var zip = ZipFile.OpenRead(Environment.GetEnvironmentVariable("XYE_G07_EAST")!))
            using (var input = zip.GetEntry("n23e121.hgt")!.Open())
            using (var output = File.Create(path)) input.CopyTo(output);
            using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                file.Position = 1; int value = file.ReadByte();
                file.Position = 1; file.WriteByte((byte)(value ^ 1));
            }
            var west = NasademHgtFile.Load(Environment.GetEnvironmentVariable("XYE_G07_WEST")!);
            var east = NasademHgtFile.Load(path);
            var result = GeoUnifiedSeamAudit.Compare(west, east);
            Assert.Equal(3601, result.BothValid);
            Assert.Equal(1, result.Conflicts);
            Assert.Equal(1, result.MaxDifferenceMeters);
            var loader = new GeoUnifiedTileLoader(new NasademTileCatalog(
                [Environment.GetEnvironmentVariable("XYE_G07_WEST")!, path]), 256L * 1024 * 1024);
            await loader.Request(23, 120);
            await loader.Request(23, 121);
            var conflict = Assert.Single(loader.SeamReports);
            Assert.Equal(1, conflict.Conflicts);
            var source = new GeoUnifiedSamples(new NasademTileCatalog(
                [Environment.GetEnvironmentVariable("XYE_G07_WEST")!, path]), "v1");
            source.Add(west); source.Add(east);
            int x = (180 + 121) * 3600, y = (90 + 24) * 3600;
            Assert.Equal(east.HeightAt(0, 3600), source.Sample(x, y).Height);
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Write))
                file.Write([0x80, 0x00]);
            var missingEast = NasademHgtFile.Load(path);
            var fallback = new GeoUnifiedSamples(new NasademTileCatalog(
                [Environment.GetEnvironmentVariable("XYE_G07_WEST")!, path]), "v2");
            fallback.Add(west); fallback.Add(missingEast);
            Assert.Equal(west.HeightAt(3600, 3600), fallback.Sample(x, y).Height);
        }
        finally { Directory.Delete(folder, true); }
    }
}
