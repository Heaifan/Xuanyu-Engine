using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Geodesy;

public sealed class test_MapGeoReference
{
    [Fact]
    public void InvalidGeographicBounds_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MapGeoReference(24, 121, 23, 122,
                MapVerticalDatum.Egm96Orthometric));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MapGeoReference(23, 122, 24, 121,
                MapVerticalDatum.Egm96Orthometric));
    }

    [Fact]
    public void NasademImport_AttachesWgs84BoundsAndEgm96HeightDatum()
    {
        string path = Path.Combine(AppContext.BaseDirectory,
            $"XYE-G01C-{Guid.NewGuid():N}-N23E121.hgt");
        try
        {
            using (var stream = File.Create(path))
                stream.SetLength(NasademTile.ByteLength);
            NasademTile tile = NasademHgtFile.Load(path);
            MapDocument map = NasademPreviewBuilder.Create(tile);

            Assert.Equal(23d, map.GeoReference!.SouthLatitudeDegrees);
            Assert.Equal(121d, map.GeoReference.WestLongitudeDegrees);
            Assert.Equal(24d, map.GeoReference.NorthLatitudeDegrees);
            Assert.Equal(122d, map.GeoReference.EastLongitudeDegrees);
            Assert.Equal(MapVerticalDatum.Egm96Orthometric,
                map.GeoReference.VerticalDatum);
        }
        finally { File.Delete(path); }
    }
}