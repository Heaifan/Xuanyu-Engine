using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07Frustum
{
    [Fact]
    public void VulkanFrustumKeepsIntersectingBoundsAndRejectsOutside()
    {
        var inside = new GeoUnifiedBounds(new(-0.5, -0.5, 0.2), new(0.5, 0.5, 0.9));
        var outside = new GeoUnifiedBounds(new(2, -0.5, 0.2), new(3, 0.5, 0.9));
        var crossing = new GeoUnifiedBounds(new(0.9, -0.5, 0.2), new(1.1, 0.5, 0.9));
        Assert.True(GeoUnifiedFrustum.Intersects(inside, Matrix4x4.Identity));
        Assert.False(GeoUnifiedFrustum.Intersects(outside, Matrix4x4.Identity));
        Assert.True(GeoUnifiedFrustum.Intersects(crossing, Matrix4x4.Identity));
    }
}
