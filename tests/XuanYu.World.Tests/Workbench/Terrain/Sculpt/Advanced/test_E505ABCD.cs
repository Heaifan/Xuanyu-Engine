using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505ABCD
{
    [Fact]
    public void TargetAndHistory_PreserveExactHeightAndEntity()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(8848, false);
        var obj = Assert.Single(wb.Scene.Objects);
        var field = wb.TerrainMap.HeightField;
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.AdvanceTerrainSculpt(1);
        wb.EndTerrainSculpt();
        Assert.Equal(8848f, field.GetHeight(50, 50));
        Assert.True(wb.CanUndoTerrain);
        var after = field.Samples.ToArray();
        wb.UndoTerrain();
        Assert.All(field.Samples.ToArray(), x => Assert.Equal(0f, x));
        Assert.True(wb.CanRedoTerrain);
        wb.RedoTerrain();
        Assert.Equal(after, field.Samples.ToArray());
        Assert.Same(obj, Assert.Single(wb.Scene.Objects));
        wb.UndoTerrain();
        wb.SelectTerrainElevationTool(false);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        Assert.False(wb.CanRedoTerrain);
        wb.ResizeLocalTerrain(100, 100, 1);
        Assert.False(wb.CanUndoTerrain);
    }

    [Fact]
    public void DirtyMesh_EqualsFullRebuildIncludingBorderNormals()
    {
        var field = new TerrainHeightField(21, 16, 2, 3);
        var geometry = TerrainMeshBuilder.Build(field);
        var mesh = TerrainRenderMeshAdapter.CreateLocal("terrain/test", geometry);
        field.SetHeight(0, 0, 12);
        field.SetHeight(1, 1, -4);
        var actual = LocalTerrainMeshUpdate.Apply(field, geometry, mesh)!;
        Assert.NotNull(actual);
        var expected = TerrainRenderMeshAdapter.CreateLocal(mesh.AssetId,
            TerrainMeshBuilder.Build(field));
        Assert.Equal(expected.Vertices.ToArray(), actual.Vertices.ToArray());
        Assert.Equal(expected.TerrainElevations.ToArray(), actual.TerrainElevations.ToArray());
        Assert.Equal(expected.TerrainShadedColors.ToArray(), actual.TerrainShadedColors.ToArray());
        Assert.Null(LocalTerrainMeshUpdate.Apply(field, geometry, actual));
    }

    [Fact]
    public void AreaElevation_UsesCoreAndFalloff_AndCanLower()
    {
        var field = new TerrainHeightField(21, 21, 1);
        var brush = new TerrainBrushSettings(8, 1, 0.5f);
        var stroke = new TerrainSculptStroke(field, brush, false, 100, true);
        stroke.MoveTo(new(10, 10));
        Assert.Equal(100f, field.GetHeight(10, 10));
        Assert.Equal(100f, field.GetHeight(14, 10));
        Assert.InRange(field.GetHeight(16, 10), 49.999f, 50.001f);
        Assert.Equal(0f, field.GetHeight(18, 10));
        new TerrainSculptStroke(field, brush, false, -20, true).MoveTo(new(10, 10));
        Assert.Equal(-20f, field.GetHeight(10, 10));
    }
}