using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06R0803BPicker
{
    private static RenderMesh Triangle(float x, float z) => new("same-asset", new float[]
    {
        x-0.25f,-0.25f,z,1,1,1,1, x+0.25f,-0.25f,z,1,1,1,1, x,0.25f,z,1,1,1,1
    });

    [Fact]
    public void PicksNearestAcrossMeshesWithSameAssetId()
    {
        var far = Triangle(0f, 0.75f);
        var off = Triangle(4f, 0.1f);
        var near = Triangle(0f, 0.25f);
        Assert.True(GeoTerrainCursorPicker.TryPick(Matrix4x4.Identity, new(400, 300),
            800, 600, [off, far, near], out var hit));
        Assert.InRange(hit.Z, 0.2499f, 0.2501f);
        Assert.True(GeoTerrainCursorPicker.TryPick(Matrix4x4.Identity, new(400, 300),
            800, 600, [near, far, off], out hit));
        Assert.InRange(hit.Z, 0.2499f, 0.2501f);
    }

    [Fact]
    public void ParallelRayOutsideBoundsAndBehindRayDoNotHit()
    {
        Assert.False(GeoTerrainCursorPicker.TryPick(Matrix4x4.Identity, new(400, 300),
            800, 600, [Triangle(4f, 0.25f), Triangle(0f, -2f)], out _));
    }

    [Fact]
    public void RayOnBoundsAndTriangleVertexStillHits()
    {
        var mesh = new RenderMesh("edge", new float[]
        {
            0,0,.5f,1,1,1,1, .5f,0,.5f,1,1,1,1, 0,.5f,.5f,1,1,1,1
        });
        Assert.True(GeoTerrainCursorPicker.TryPick(Matrix4x4.Identity, new(400, 300),
            800, 600, [mesh], out var hit));
        Assert.InRange(hit.Z, 0.4999f, 0.5001f);
    }
}