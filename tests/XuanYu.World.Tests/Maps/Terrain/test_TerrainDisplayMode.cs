
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainDisplayMode
{
    [Fact]
    public void BothModes_KeepVertexPositionsAndChangeColors()
    {
        var field = new TerrainHeightField(3, 3, 10f);
        field.SetHeight(1, 1, 18f);
        field.SetHeight(2, 2, 28f);
        TerrainMeshGeometry geometry = TerrainMeshBuilder.Build(field);

        var standard = TerrainRenderMeshAdapter.Create(geometry);
        var elevation = TerrainRenderMeshAdapter.Create(
            geometry, TerrainDisplayMode.Elevation);

        Assert.Equal(standard.VertexCount, elevation.VertexCount);
        ReadOnlySpan<float> a = standard.Vertices.Span;
        ReadOnlySpan<float> b = elevation.Vertices.Span;
        bool differentColor = false;

        for (int i = 0; i < a.Length; i += 7)
        {
            for (int k = 0; k < 3; k++)
                Assert.Equal(a[i + k], b[i + k]);
            for (int k = 3; k < 6; k++)
                differentColor |= MathF.Abs(a[i + k] - b[i + k]) > 0.001f;
            Assert.Equal(1f, b[i + 6]);
        }

        Assert.True(differentColor);
    }

    [Fact]
    public void FlatTerrain_ElevationColorsRemainFinite()
    {
        var geometry = TerrainMeshBuilder.Build(
            new TerrainHeightField(2, 2, 10f));
        var mesh = TerrainRenderMeshAdapter.Create(
            geometry, TerrainDisplayMode.Elevation);

        foreach (float value in mesh.Vertices.Span)
            Assert.True(float.IsFinite(value));
    }
}
