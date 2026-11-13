
using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class TestE403TerrainBridge
{
    [Fact]
    public void PreviewMap_ContainsHeightVariation()
    {
        var map = TerrainPreviewMap.Create();
        var field = map.HeightField;

        Assert.Equal(256f, map.WidthMeters);
        Assert.Equal(256f, map.LengthMeters);
        Assert.NotEqual(
            field.GetHeight(0, 0),
            field.GetHeight(64, 64));
    }

    [Fact]
    public void Adapter_ProducesExistingRenderMeshFormat()
    {
        var session = new TerrainPreviewSession();
        var draw = Assert.Single(
            session.Scene.ViewportDraws(1200, 800));

        Assert.Equal(98304, draw.Mesh.VertexCount);
        Assert.Equal(32768, draw.Mesh.TriangleCount);
        Assert.Equal(
            98304 * 7,
            draw.Mesh.Vertices.Length);

        ReadOnlySpan<float> data = draw.Mesh.Vertices.Span;
        Assert.Equal(1f, data[6]);
        Assert.True(float.IsFinite(data[0]));
        Assert.InRange(data[3], 0f, 1f);
        Assert.NotEqual(Matrix4x4.Identity, draw.ModelToClip);
    }

    [Fact]
    public void CameraChanges_DoNotRebuildTerrainMesh()
    {
        var session = new TerrainPreviewSession();
        var camera = session.Scene.Camera!;

        var before = Assert.Single(
            session.Scene.ViewportDraws(1200, 800));

        float originalHeight =
            session.Map.HeightField.GetHeight(64, 64);

        camera.Orbit(120f, 45f);
        camera.Zoom(2f);

        var after = Assert.Single(
            session.Scene.ViewportDraws(800, 600));

        Assert.Same(before.Mesh, after.Mesh);
        Assert.NotEqual(before.ModelToClip, after.ModelToClip);
        Assert.Equal(originalHeight,
            session.Map.HeightField.GetHeight(64, 64));
    }
}
