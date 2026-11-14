
using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainCameraCollision
{
    [Fact]
    public void Zoom_StopsBeforeFlatTerrain()
    {
        var field = new TerrainHeightField(2, 2, 200f);
        var preview = new TerrainPreviewSession(
            new MapDocument("平面", field));
        ViewportCamera camera = preview.Scene.Camera!;
        camera.Frame(new Vector3(100f, 100f, 0f), 5f, 1.4f);

        camera.Zoom(1000f);
        Assert.InRange(camera.Position.Z, 0.499f, 0.501f);

        float before = camera.Position.Z;
        camera.Zoom(1000f);
        Assert.InRange(MathF.Abs(camera.Position.Z - before), 0f, 0.001f);
    }

    [Fact]
    public void OrbitAndPan_CannotEnterTerrain()
    {
        var preview = new TerrainPreviewSession(
            new MapDocument("平面", new TerrainHeightField(2, 2, 200f)));
        ViewportCamera camera = preview.Scene.Camera!;
        camera.Frame(new Vector3(100f, 100f, 0f), 5f, 1.4f);
        camera.Zoom(1000f);
        Vector3 before = camera.Position;

        camera.Orbit(0f, -400f);
        Assert.Equal(before, camera.Position);

        camera.Pan(0f, -2000f, 600);
        Assert.Equal(before, camera.Position);
    }

    [Fact]
    public void SurfaceHeight_MatchesTerrainTriangle()
    {
        var field = new TerrainHeightField(2, 2, 200f);
        field.SetHeight(1, 1, 80f);
        var preview = new TerrainPreviewSession(
            new MapDocument("斜坡", field));
        ViewportCamera camera = preview.Scene.Camera!;
        camera.Frame(new Vector3(100f, 100f, 0f), 5f, 1.4f);

        Vector3 p = camera.Position;
        float triangleHeight = 80f * p.Y / 200f;
        Assert.InRange(p.Z - triangleHeight, 0.499f, 0.501f);
    }

    [Fact]
    public void ScaledTerrain_UsesDisplayHeight()
    {
        var field = new TerrainHeightField(2, 2, 2000f);
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++)
                field.SetHeight(x, y, 40f);
        var preview = new TerrainPreviewSession(
            new MapDocument("缩放地图", field));
        ViewportCamera camera = preview.Scene.Camera!;
        camera.Frame(new Vector3(128f, 128f, 5.12f), 5f, 1.4f);

        camera.Zoom(1000f);
        Assert.InRange(camera.Position.Z, 5.619f, 5.621f);
    }
}
