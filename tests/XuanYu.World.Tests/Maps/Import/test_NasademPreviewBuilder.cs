
using System.Buffers.Binary;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestNasademPreviewBuilder
{
    [Fact]
    public void GeoReference_ConvertsLocalMetersAndCoordinates()
    {
        string path = CreateTile();
        try
        {
            var geo = new NasademGeoReference(NasademHgtFile.Load(path));
            Assert.InRange(geo.WidthMeters, 100000, 105000);
            Assert.InRange(geo.LengthMeters, 110000, 112000);
            var point = geo.GeoAt(geo.WidthMeters / 2,
                geo.LengthMeters / 2);
            Assert.Equal(24.5, point.Latitude, 6);
            Assert.Equal(121.5, point.Longitude, 6);
            var local = geo.LocalAt(point.Latitude, point.Longitude);
            Assert.Equal(geo.WidthMeters / 2, local.EastMeters, 5);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Preview_ResamplesOriginalTileCorners()
    {
        string path = CreateTile();
        try
        {
            WriteSample(path, 0, 0, 500);
            WriteSample(path, 3600, 3600, -200);
            var tile = NasademHgtFile.Load(path);
            MapDocument map = NasademPreviewBuilder.Create(tile);
            var field = map.HeightField;
            var geo = new NasademGeoReference(tile);

            Assert.Equal("NASADEM_N24E121", map.Name);
            Assert.Equal(129, field.SamplesY);
            Assert.InRange(field.SamplesX, 115, 125);
            Assert.Equal(500f, field.GetHeight(0, 0));
            Assert.Equal(-200f, field.GetHeight(
                field.SamplesX - 1, field.SamplesY - 1));
            Assert.InRange(Math.Abs(map.WidthMeters - geo.WidthMeters),
                0, field.SpacingMeters / 2 + 1);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Preview_DoesNotPropagateNoDataValue()
    {
        string path = CreateTile();
        try
        {
            WriteSample(path, 0, 0, short.MinValue);
            var tile = NasademHgtFile.Load(path);
            var map = NasademPreviewBuilder.Create(tile);

            Assert.Equal(1, tile.NoDataCount);
            Assert.Equal(0f, map.HeightField.GetHeight(0, 0));
            Assert.Null(tile.HeightAt(0, 0));
        }
        finally { File.Delete(path); }
    }

    private static string CreateTile()
    {
        string path = Path.Combine(AppContext.BaseDirectory,
            $"XYE-{Guid.NewGuid():N}-n24e121.hgt");
        using var stream = File.Create(path);
        stream.SetLength(NasademTile.ByteLength);
        return path;
    }

    private static void WriteSample(
        string path, int column, int rowFromSouth, short height)
    {
        using var stream = File.OpenWrite(path);
        stream.Position = ((long)(3600 - rowFromSouth) *
            NasademTile.Side + column) * 2;
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(bytes, height);
        stream.Write(bytes);
    }
}
