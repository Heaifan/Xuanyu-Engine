using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G03EVisibleSurface
{
    [Fact]
    public void VisibleTriangleHeight_IsUsedForCameraClearance()
    {
        var mesh = new RenderMesh("terrain/test", new float[]
        {
            0, 0, 120, 0, 1, 0, 1,
            10, 0, 120, 0, 1, 0, 1,
            0, 10, 120, 0, 1, 0, 1
        });
        Assert.Equal(120d, GeoTerrainMeshSurface.HeightAt(mesh, 2, 2)!.Value);
        Assert.Null(GeoTerrainMeshSurface.HeightAt(mesh, 20, 20));
        var location = new GeoCoordinate(24.5d, 121.5d, 0d);
        var camera = new GlobalCamera(location, 20d);
        camera.SetTerrainCollision(_ => 120d);
        Assert.True(camera.RestoreTerrainClearance());
        Assert.True(Wgs84.ToGeo(camera.PositionEcef).HeightMeters >= 123d);
    }

    [Fact]
    public void GizmoSnapsToCardinalDirectionsAndStaysAboveSurface()
    {
        var camera = new GlobalCamera(new GeoCoordinate(24.5d, 121.5d, 0d), 20d);
        camera.SetTerrainCollision(_ => 120d);
        camera.SnapAxis(Vector3.UnitX);
        Assert.InRange(camera.YawRadians, 1.56d, 1.58d);
        Assert.True(Wgs84.ToGeo(camera.PositionEcef).HeightMeters >= 123d);
        camera.SnapAxis(Vector3.UnitZ);
        Assert.InRange(camera.PitchRadians, 1.55d, 1.57d);
    }
}