using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_TerrainPointerZoom
{
    [Fact]
    public void LegacyMap_ZoomAtCursor_UsesExistingCameraAndTerrain()
    {
        var session = new TerrainPreviewSession();
        var camera = session.Scene.Camera!;
        const uint width = 1000, height = 700;
        Vector2 pixel = new(width / 2f, height / 2f);
        Assert.True(session.TryPick(pixel, width, height, out _));
        Vector3 before = camera.Position;
        session.ZoomAtCursor(1f, pixel, width, height);
        Assert.NotEqual(before, camera.Position);
        Assert.True(float.IsFinite(camera.Position.X));
        Assert.True(float.IsFinite(camera.Position.Y));
        Assert.True(float.IsFinite(camera.Position.Z));
    }
}