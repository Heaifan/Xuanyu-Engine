using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E505AV03Hillshade
{
    [Fact]
    public void LocalEdit_PreservesBaseColorsAndRefreshesShade()
    {
        var field = new TerrainHeightField(5, 5, 1f, 1f);
        var geometry = TerrainMeshBuilder.Build(field);
        var initial = TerrainRenderMeshAdapter.CreateLocal("terrain/test", geometry);
        Assert.Equal(initial.VertexCount * 3, initial.TerrainShadedColors.Length);
        field.SetHeight(2, 2, 4f);
        var edited = LocalTerrainMeshUpdate.Apply(field, geometry, initial);
        Assert.NotNull(edited);
        Assert.Equal(initial.VertexCount, edited!.VertexCount);
        Assert.Equal(edited.VertexCount * 3, edited.TerrainShadedColors.Length);
        var before = initial.Vertices.Span;
        var after = edited.Vertices.Span;
        var oldShade = initial.TerrainShadedColors.Span;
        var newShade = edited.TerrainShadedColors.Span;
        bool adjacentShadeChanged = false, raised = false;
        for (int i = 0; i < edited.VertexCount; i++)
        {
            int v = i * 7, c = i * 3;
            Assert.Equal(after[v + 2], edited.TerrainElevations.Span[i]);
            Assert.Equal(1f, after[v + 6]);
            if (after[v + 2] != before[v + 2])
            {
                Assert.Equal(4f, after[v + 2]);
                raised = true;
                continue;
            }
            Assert.True(before.Slice(v + 3, 3).SequenceEqual(after.Slice(v + 3, 3)));
            adjacentShadeChanged |= !oldShade.Slice(c, 3).SequenceEqual(newShade.Slice(c, 3));
        }
        Assert.True(raised);
        Assert.True(adjacentShadeChanged);
        Assert.True(oldShade.Slice(0, 3).SequenceEqual(newShade.Slice(0, 3)));
        field.SetHeight(2, 2, 0f);
        var restored = LocalTerrainMeshUpdate.Apply(field, geometry, edited);
        Assert.NotNull(restored);
        Assert.True(initial.Vertices.Span.SequenceEqual(restored!.Vertices.Span));
        Assert.True(initial.TerrainShadedColors.Span.SequenceEqual(restored.TerrainShadedColors.Span));
    }
}