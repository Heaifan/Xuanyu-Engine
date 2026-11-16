using System.IO.Compression;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G01CMapPersistence
{
    private static readonly MapGeoReference Geo = new(23, 121, 24, 122,
        MapVerticalDatum.Egm96Orthometric);

    [Fact]
    public void SmallJsonMap_RoundTripsGeoReference()
    {
        var map = new MapDocument("JSON 地图", new TerrainHeightField(3, 3, 2f),
            geoReference: Geo);
        string path = NewPath();
        try
        {
            MapFile.Save(map, path);
            Assert.Equal(Geo, MapFile.Load(path).GeoReference);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void BinaryMap_RoundTripsGeoReferenceAndWater()
    {
        var field = new TerrainHeightField(3, 3, 1f);
        field.SetHeight(1, 2, 385f);
        var water = new TerrainWaterMask(3, 3, new byte[9]);
        var map = new MapDocument("二进制地图", field, water, Geo);
        string path = NewPath();
        try
        {
            MapFile.Save(map, path);
            using (var file = File.OpenRead(path))
            {
                Span<byte> magic = stackalloc byte[8];
                file.ReadExactly(magic);
                Assert.True(magic.SequenceEqual("XYEMAP04"u8));
            }
            MapDocument loaded = MapFile.Load(path);
            Assert.Equal(Geo, loaded.GeoReference);
            Assert.NotNull(loaded.WaterMask);
            Assert.Equal(385f, loaded.HeightField.GetHeight(1, 2));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void LegacyBinary03_StillLoadsWithoutGeoReference()
    {
        string path = NewPath();
        try
        {
            using (var file = File.Create(path))
            {
                file.Write("XYEMAP03"u8);
                using var gzip = new GZipStream(file, CompressionLevel.Fastest);
                using var writer = new BinaryWriter(gzip);
                writer.Write(1);
                writer.Write("旧地图");
                writer.Write(2);
                writer.Write(2);
                writer.Write(1f);
                writer.Write(1f);
                for (int i = 0; i < 4; i++) writer.Write((float)i);
                writer.Write(false);
            }
            MapDocument loaded = MapFile.Load(path);
            Assert.Null(loaded.GeoReference);
            Assert.Equal(3f, loaded.HeightField.GetHeight(1, 1));
        }
        finally { File.Delete(path); }
    }

    private static string NewPath() => Path.Combine(AppContext.BaseDirectory,
        $"XYE-G01C-{Guid.NewGuid():N}.xymap");
}