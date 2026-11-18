using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05TileCatalog
{
    [Fact]
    public void AdjacentFiles_LoadExactSamplesThroughExistingElevationSource()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        west.SetHeight(1800, 1800, 123);
        east.SetHeight(1800, 1800, 456);
        west.SetHeight(3600, 1800, 789);
        east.SetHeight(0, 1800, 789);
        var catalog = new NasademTileCatalog([west.Path, east.Path]);

        Assert.Equal(2, catalog.Count);
        Assert.Contains((24, 120), catalog.Coordinates);
        Assert.Contains((24, 121), catalog.Coordinates);
        Assert.Null(catalog.Load(24, 122));
        var source = new NasademElevationSource(
            catalog.Load(24, 120)!, catalog.Load(24, 121)!);
        Assert.Equal(123d, source.Sample(24.5, 120.5)!.Value);
        Assert.Equal(456d, source.Sample(24.5, 121.5)!.Value);
        Assert.Equal(789d, source.Sample(24.5, 121)!.Value);
        Assert.Null(source.Sample(24.5, 122.5));
    }

    [Fact]
    public void DuplicateCoordinates_AreRejected()
    {
        using var a = new G02BHgtFixture("N24E121");
        using var b = new G02BHgtFixture("N24E121");
        Assert.Throws<InvalidDataException>(() =>
            new NasademTileCatalog([a.Path, b.Path]));
    }

    [Fact]
    public void ZipCatalog_UsesInnerHgtNameAndLoadsOnRequest()
    {
        using var hgt = new G02BHgtFixture("S01W002");
        hgt.SetHeight(1800, 1800, 314);
        string zip = Path.Combine(AppContext.BaseDirectory,
            $"XYE-G05-{Guid.NewGuid():N}.zip");
        try
        {
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(hgt.Path, "NASADEM_HGT_s01w002.hgt");
            var catalog = new NasademTileCatalog([zip]);
            Assert.Contains((-1, -2), catalog.Coordinates);
            Assert.Equal((short)314, catalog.Load(-1, -2)!.HeightAt(1800, 1800));
        }
        finally { File.Delete(zip); }
    }

    [Fact]
    public void Scan_IgnoresOtherExtensionsAndRejectsInvalidHgtLength()
    {
        string directory = Path.Combine(AppContext.BaseDirectory,
            $"XYE-G05-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "readme.txt"), "test");
            string path = Path.Combine(directory, "N24E121.HGT");
            using (var stream = File.Create(path))
                stream.SetLength(NasademTile.ByteLength);
            Assert.Equal(1, NasademTileCatalog.Scan(directory).Count);
            File.WriteAllBytes(path, [0, 1]);
            Assert.Throws<InvalidDataException>(() =>
                NasademTileCatalog.Scan(directory));
        }
        finally { Directory.Delete(directory, true); }
    }
}