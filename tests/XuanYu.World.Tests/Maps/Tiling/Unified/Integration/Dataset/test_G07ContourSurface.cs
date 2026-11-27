using System.Numerics;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ContourSurface
{
    [Fact]
    public void MajorContour_IntersectsTheActualTriangle()
    {
        var hit = new GeoUnifiedSurfaceHit(new Vector3(4, 3, 80), new Vector3(0, 0, 0),
            new Vector3(10, 0, 200), new Vector3(0, 10, 0), new Vector3(0, 200, 0), 80);
        Assert.True(hit.TryContour(100, out int elevation, out var point, out var tangent));
        Assert.Equal(100, elevation);
        Assert.Equal(5f, point.X, 4);
        Assert.Equal(3f, point.Y, 4);
        Assert.Equal(100f, point.Z, 4);
        Assert.Equal(0f, tangent.X, 4);
        Assert.False(hit.TryContour(500, out _, out _, out _));
        Assert.False((hit with { Heights = Vector3.Zero }).TryContour(100, out _, out _, out _));
    }
}