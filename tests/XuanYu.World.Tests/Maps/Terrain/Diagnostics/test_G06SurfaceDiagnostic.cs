using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06NearbySurface
{
    [Fact]
    public void TinyOutsideOffset_UsesExistingEdgeHeight()
    {
        var surface = new GeoContourSurface(Triangle());
        Assert.InRange(surface.HeightAt(10.1f, 0f, center: true)!.Value,
            19.99f, 20.01f);
        Assert.InRange(surface.HeightAt(5.08f, 5.08f)!.Value,
            24.99f, 25.01f);
        Assert.Equal(2, surface.RecoveryCount);
        Assert.Equal(0, surface.CenterMissCount);
        Assert.Equal(0, surface.TriangleMissCount);
    }

    [Fact]
    public void DistantOutsidePoint_IsNotInvented()
    {
        var surface = new GeoContourSurface(Triangle());
        Assert.Null(surface.HeightAt(15f, 0f, center: true));
        Assert.Equal(0, surface.RecoveryCount);
        Assert.Equal(1, surface.CenterMissCount);
        Assert.Equal(1, surface.BoundsMissCount);
    }

    [Fact]
    public void ActualGapBetweenTriangles_RemainsEmpty()
    {
        float[] data =
        [
            0, 0, 10, 1, 1, 1, 1,
            10, 0, 20, 1, 1, 1, 1,
            0, 10, 30, 1, 1, 1, 1,
            30, 30, 40, 1, 1, 1, 1,
            40, 30, 40, 1, 1, 1, 1,
            30, 40, 40, 1, 1, 1, 1
        ];
        var surface = new GeoContourSurface(new RenderMesh("test/g06-gap", data));
        Assert.Null(surface.HeightAt(20f, 20f, center: true));
        Assert.Equal(0, surface.RecoveryCount);
        Assert.Equal(1, surface.CenterMissCount);
    }

    private static RenderMesh Triangle()
    {
        float[] vertices =
        [
            0, 0, 10, 1, 1, 1, 1,
            10, 0, 20, 1, 1, 1, 1,
            0, 10, 30, 1, 1, 1, 1
        ];
        return new RenderMesh("test/g06-near", vertices);
    }
}