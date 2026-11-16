using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainShadingField
{
    [Fact]
    public void DenseBorder_ShadesFromSameCoarseLattice()
    {
        var field = CreateHeights(301, 301);
        var shading = new TerrainShadingField(field, (-100f, 100f));
        RenderMesh mesh = TerrainTileMeshBuilder.Create(field, 0, 0, 20,
            heightRange: (-100f, 100f), shading: shading);
        Vector3 first = Color(mesh, 0f, 0f);
        Vector3 last = Color(mesh, 0f, 40f);
        for (int sample = 5; sample < 20; sample += 5)
        {
            Vector3 expected = Vector3.Lerp(first, last, sample / 20f);
            Vector3 actual = Color(mesh, 0f, sample * 2f);
            Assert.InRange(Vector3.Distance(expected, actual), 0f, 0.00001f);
        }
    }

    [Fact]
    public void MixedLods_ShareExactColorsOnAdjacentBorder()
    {
        var field = CreateHeights(601, 301);
        var shading = new TerrainShadingField(field, (-100f, 100f));
        RenderMesh a = TerrainTileMeshBuilder.Create(field, 0, 0, 5,
            shading: shading);
        RenderMesh b = TerrainTileMeshBuilder.Create(field, 300, 0, 20,
            shading: shading);
        for (int y = 0; y <= 300; y += 5)
            Assert.Equal(Color(a, 600f, y * 2f), Color(b, 600f, y * 2f));
    }

    [Fact]
    public void WaterMask_PreservesBlueSea()
    {
        var field = new TerrainHeightField(301, 301, 2f);
        byte[] mask = new byte[field.SampleCount];
        for (int y = 0; y < 301; y++)
            for (int x = 0; x < 150; x++) mask[y * 301 + x] = 255;
        var shading = new TerrainShadingField(field, (0f, 0f));
        RenderMesh mesh = TerrainTileMeshBuilder.Create(field, 0, 0, 20,
            new TerrainWaterMask(301, 301, mask), shading: shading);
        Vector3 sea = Color(mesh, 0f, 40f);
        Vector3 land = Color(mesh, 600f, 40f);
        Assert.True(sea.Z > sea.Y);
        Assert.True(land.Y > land.Z);
    }

    [Fact]
    public void Shading_DoesNotModifyGeometry()
    {
        var field = CreateHeights(301, 301);
        RenderMesh original = TerrainTileMeshBuilder.Create(field, 0, 0, 20);
        RenderMesh shaded = TerrainTileMeshBuilder.Create(field, 0, 0, 20,
            shading: new TerrainShadingField(field, (-100f, 100f)));
        Assert.Equal(original.VertexCount, shaded.VertexCount);
        bool colorsChanged = false;
        ReadOnlySpan<float> before = original.Vertices.Span;
        ReadOnlySpan<float> after = shaded.Vertices.Span;
        for (int i = 0; i < before.Length; i += 7)
        {
            for (int j = 0; j < 3; j++) Assert.Equal(before[i + j], after[i + j]);
            colorsChanged |= MathF.Abs(before[i + 3] - after[i + 3]) > 0.0001f;
        }
        Assert.True(colorsChanged);
    }

    private static TerrainHeightField CreateHeights(int width, int height)
    {
        var field = new TerrainHeightField(width, height, 2f);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                field.SetHeight(x, y, 26f * MathF.Sin(y * 0.14f) +
                    15f * MathF.Cos(x * 0.09f));
        return field;
    }

    private static Vector3 Color(RenderMesh mesh, float x, float y)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        for (int i = 0; i < data.Length; i += 7)
            if (data[i] == x && data[i + 1] == y)
                return new Vector3(data[i + 3], data[i + 4], data[i + 5]);
        throw new InvalidOperationException($"未找到地形顶点 ({x}, {y})");
    }
}