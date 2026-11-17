using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G03DLodHysteresis
{
    private static GeoTileView View(int height)
    {
        var center = Wgs84.ToEcef(new GeoCoordinate(24.5d, 121.5d, 0d));
        var eye = Wgs84.ToEcef(new GeoCoordinate(24.5d, 121.5d, 150_000d));
        return new GeoTileView(eye, center, Math.PI / 3d,
            height * 16 / 9, height);
    }

    [Fact]
    public void MildResolutionChange_DoesNotDropMoreDetailThanColdSelection()
    {
        var coverage = new GeoTileBounds(24d, 121d, 25d, 122d);
        var warm = new GeoTileSelector(200_000d, 4d, maxLevel: 13, maxTiles: 256);
        warm.Select(View(1100), coverage);
        var stabilized = warm.Select(View(1000), coverage);
        var cold = new GeoTileSelector(200_000d, 4d, 13, 256)
            .Select(View(1000), coverage);

        Assert.True(stabilized.Count >= cold.Count);
        Assert.Equal(stabilized, warm.Select(View(1000), coverage));
        Assert.InRange(stabilized.Count, 1, 256);
        Assert.Equal(stabilized.Count, stabilized.Distinct().Count());
    }
}