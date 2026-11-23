using Xunit;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06HudScale
{
    [Theory]
    [InlineData(1d, 1d, 100d, 100d)]
    [InlineData(4d, 1d, 500d, 125d)]
    [InlineData(20d, 1d, 2000d, 100d)]
    [InlineData(1d, 2d, 200d, 100d)]
    public void ChoosesReadableScale(double metersPerPixel, double dpi,
        double meters, double widthDip)
    {
        GeoTerrainHudScale scale = Assert.IsType<GeoTerrainHudScale>(
            GeoTerrainHudScale.From(metersPerPixel, dpi));
        Assert.Equal(meters, scale.Meters);
        Assert.Equal(widthDip, scale.WidthDip, 6);
    }

    [Fact]
    public void MissingScaleIsNotInvented()
    {
        Assert.Null(GeoTerrainHudScale.From(null, 1d));
        Assert.Null(GeoTerrainHudScale.From(0d, 1d));
    }
}