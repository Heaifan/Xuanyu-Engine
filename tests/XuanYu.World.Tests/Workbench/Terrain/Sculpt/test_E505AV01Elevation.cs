using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505AV01Elevation
{
    [Fact]
    public void RemotePeak_DoesNotRecolorUnchangedFlatGround()
    {
        var field = new TerrainHeightField(3, 3, 1f);
        var before = TerrainRenderMeshAdapter.Create(TerrainMeshBuilder.Build(field));
        field.SetHeight(2, 2, 3000f);
        var after = TerrainRenderMeshAdapter.Create(TerrainMeshBuilder.Build(field));
        Assert.Equal(Color(before.Vertices.Span), Color(after.Vertices.Span));
    }

    [Fact]
    public void DifferentAbsoluteHeights_HaveDifferentFlatColors()
    {
        var field = new TerrainHeightField(2, 2, 1f);
        var low = TerrainRenderMeshAdapter.Create(TerrainMeshBuilder.Build(field));
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            field.SetHeight(x, y, 3000f);
        var high = TerrainRenderMeshAdapter.Create(TerrainMeshBuilder.Build(field));
        Assert.NotEqual(Color(low.Vertices.Span), Color(high.Vertices.Span));
        Assert.Equal(3000f, high.Vertices.Span[2]);
    }

    [Fact]
    public void CachedShading_MatchesDirectFlatColorAtSameElevation()
    {
        var field = new TerrainHeightField(2, 2, 1f);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            field.SetHeight(x, y, 40f);
        var mesh = TerrainRenderMeshAdapter.Create(TerrainMeshBuilder.Build(field));
        var cache = new TerrainShadingField(field, (40f, 40f));
        Assert.Equal(Color(mesh.Vertices.Span), cache.Sample(0, 0));
        var otherRange = new TerrainShadingField(field, (-100f, 3000f));
        Assert.Equal(cache.Sample(0, 0), otherRange.Sample(0, 0));
    }

    private static Vector3 Color(ReadOnlySpan<float> vertex) =>
        new(vertex[3], vertex[4], vertex[5]);
}