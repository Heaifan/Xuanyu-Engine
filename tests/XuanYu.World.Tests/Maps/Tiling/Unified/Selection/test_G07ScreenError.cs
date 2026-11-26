using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ScreenError
{
    [Fact]
    public void PerspectiveMetricUsesActualHeightDistanceAndHysteresis()
    {
        double pixels = GeoUnifiedScreenError.Estimate(10, 1000, 1000, Math.PI / 2);
        Assert.Equal(5, pixels, 10);
        Assert.True(double.IsPositiveInfinity(GeoUnifiedScreenError.Estimate(10, 1000, 0, Math.PI / 2)));
        Assert.True(GeoUnifiedScreenError.ShouldRefine(2.6, false));
        Assert.False(GeoUnifiedScreenError.ShouldRefine(2.4, false));
        Assert.True(GeoUnifiedScreenError.ShouldRefine(1.6, true));
        Assert.False(GeoUnifiedScreenError.ShouldRefine(1.4, true));
    }
}
