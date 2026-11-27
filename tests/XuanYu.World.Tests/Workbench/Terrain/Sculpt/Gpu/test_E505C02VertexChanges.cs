using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E505C02VertexChanges
{
    private const string Id = "terrain/local/test";

    [Fact]
    public void LocalChanges_IncludeSkippedEditsAndUndo()
    {
        var field = new TerrainHeightField(41, 41, 1);
        var geometry = TerrainMeshBuilder.Build(field);
        var before = TerrainRenderMeshAdapter.CreateLocal(Id, geometry);
        field.SetHeight(2, 2, 12);
        var middle = LocalTerrainMeshUpdate.Apply(field, geometry, before)!;
        field.SetHeight(38, 38, -5);
        var after = LocalTerrainMeshUpdate.Apply(field, geometry, middle)!;
        Assert.True(LocalTerrainVertexChanges.CanReuse(before, after));
        AssertDelta(before, after);
        AssertDelta(after, before);
        Assert.Empty(LocalTerrainVertexChanges.Find(after, after));
    }

    [Fact]
    public void Reuse_RequiresSameLocalAssetAndVertexCount()
    {
        var geometry = TerrainMeshBuilder.Build(new TerrainHeightField(5, 5, 1));
        var first = TerrainRenderMeshAdapter.CreateLocal(Id, geometry);
        Assert.False(LocalTerrainVertexChanges.CanReuse(first,
            TerrainRenderMeshAdapter.CreateLocal("terrain/local/other", geometry)));
        Assert.False(LocalTerrainVertexChanges.CanReuse(first,
            TerrainRenderMeshAdapter.CreateLocal(Id,
                TerrainMeshBuilder.Build(new TerrainHeightField(6, 5, 1)))));
        var dem = TerrainRenderMeshAdapter.CreateLocal("terrain/dem/test", geometry);
        Assert.False(LocalTerrainVertexChanges.CanReuse(dem, dem));
    }

    private static void AssertDelta(RenderMesh before, RenderMesh after)
    {
        var ranges = LocalTerrainVertexChanges.Find(before, after);
        Assert.NotEmpty(ranges);
        Assert.InRange(ranges.Sum(r => r.Count), 1, after.VertexCount - 1);
        var vertices = before.Vertices.ToArray();
        var heights = before.TerrainElevations.ToArray();
        var colors = before.TerrainShadedColors.ToArray();
        foreach (var (first, count) in ranges)
        {
            Assert.Equal(0, first % 3);
            Assert.Equal(0, count % 3);
            after.Vertices.Span.Slice(first * 7, count * 7)
                .CopyTo(vertices.AsSpan(first * 7, count * 7));
            after.TerrainElevations.Span.Slice(first, count)
                .CopyTo(heights.AsSpan(first, count));
            after.TerrainShadedColors.Span.Slice(first * 3, count * 3)
                .CopyTo(colors.AsSpan(first * 3, count * 3));
        }
        Assert.Equal(after.Vertices.ToArray(), vertices);
        Assert.Equal(after.TerrainElevations.ToArray(), heights);
        Assert.Equal(after.TerrainShadedColors.ToArray(), colors);
    }
}