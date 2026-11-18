using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05HeightPyramidFile
{
    [Fact]
    public void RealZip_PreparesTwoIndependentlyReadableFiles()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1800, 1800, 1234);
        hgt.SetHeight(60, 120, short.MinValue);
        string dir = Path.Combine(AppContext.BaseDirectory, $"XYE-Pyramid-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            string zip = Path.Combine(dir, "source.zip");
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(hgt.Path, "NASADEM_HGT_n24e121.hgt");
            var catalog = new NasademTileCatalog([zip]);
            var entry = Assert.Single(catalog.Entries);
            var paths = NasademPyramidPreparation.Prepare(entry, Path.Combine(dir, "derived"));
            Assert.True(File.Exists(zip));
            File.Delete(zip);
            var far = NasademOverviewFile.Read(paths.Far, entry.Info, NasademOverviewLevel.Far);
            var middle = NasademOverviewFile.Read(paths.Middle, entry.Info, NasademOverviewLevel.Middle);
            Assert.Equal(7462L, new FileInfo(paths.Far).Length);
            Assert.Equal(181222L, new FileInfo(paths.Middle).Length);
            Assert.Equal((short)1234, far.HeightAt(30, 30));
            Assert.Equal((short)1234, middle.HeightAt(150, 150));
            Assert.Null(far.HeightAt(1, 2));
            Assert.Equal(entry.Info, middle.Info);
            Assert.Throws<InvalidDataException>(() => NasademOverviewFile.Read(
                paths.Far, entry.Info, NasademOverviewLevel.Middle));
            var wrong = new NasademHgtInfo("N24E122", 24, 122);
            Assert.Throws<InvalidDataException>(() => NasademOverviewFile.Read(
                paths.Far, wrong, NasademOverviewLevel.Far));
            using (var stream = File.OpenWrite(paths.Far)) stream.SetLength(20);
            Assert.Throws<InvalidDataException>(() => NasademOverviewFile.Read(
                paths.Far, entry.Info, NasademOverviewLevel.Far));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}