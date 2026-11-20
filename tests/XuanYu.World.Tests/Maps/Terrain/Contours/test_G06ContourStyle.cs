using Xunit;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06ContourStyle
{
    [Theory]
    [InlineData(1000d, 20)]
    [InlineData(2500d, 20)]
    [InlineData(7500d, 50)]
    [InlineData(20000d, 100)]
    [InlineData(60000d, 200)]
    [InlineData(180000d, 500)]
    [InlineData(1999999d, 500)]
    [InlineData(2000000d, 500)]
    public void Distance_SelectsOneOfSixIntervals(double distance, int expected)
    {
        Assert.Equal(expected, GeoContourStyle.AtDistance(distance).IntervalMeters);
    }

    [Fact]
    public void Opacity_FadesAcrossSixDistanceBands()
    {
        double[] distances = [1000d, 3000d, 8000d, 21000d, 70000d, 3000000d];
        float previous = 1f;
        foreach (double distance in distances)
        {
            float opacity = GeoContourStyle.AtDistance(distance).Opacity;
            Assert.True(opacity < previous);
            previous = opacity;
        }
    }

    [Fact]
    public void LineWidth_ChangesWithZoomButStaysStableWithinABand()
    {
        var near = GeoContourStyle.AtDistance(1000d);
        var sameBand = GeoContourStyle.AtDistance(1050d);
        var far = GeoContourStyle.AtDistance(200000d);
        Assert.Equal(near.WidthMeters, sameBand.WidthMeters);
        Assert.True(far.WidthMeters > near.WidthMeters);
        Assert.True(near.WidthMeters > 0f);
    }

    [Fact]
    public void Interval_TransitionsSmoothlyWithoutChangingDrawCount()
    {
        var before = GeoContourStyle.AtDistance(2_250d);
        var middle = GeoContourStyle.AtDistance(2_500d);
        var after = GeoContourStyle.AtDistance(2_750d);
        Assert.Equal((20, 50), (before.IntervalMeters, before.NextIntervalMeters));
        Assert.Equal(0f, before.IntervalBlend);
        Assert.Equal(0.5f, middle.IntervalBlend);
        Assert.Equal(1f, after.IntervalBlend);
    }
}
