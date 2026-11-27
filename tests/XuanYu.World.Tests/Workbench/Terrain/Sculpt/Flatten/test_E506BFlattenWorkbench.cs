using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E506BFlattenWorkbench
{
    [Fact]
    public void Flatten_UsesPickedHeightAndExistingHistory()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.ActiveCamera!.SnapAxis(Vector3.UnitZ);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(8f, 1f, 1f));
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(100f, false);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        var field = wb.TerrainMap.HeightField;
        var before = field.Samples.ToArray();
        Assert.Equal(100f, field.GetHeight(50, 50));
        Assert.True(field.GetHeight(53, 50) < 100f);
        wb.SetTerrainElevation(999f, true);
        wb.SelectTerrainFlattenTool();
        Assert.True(wb.IsTerrainFlattenTool);
        Assert.False(wb.IsTerrainElevationTool);
        Assert.False(wb.IsTerrainSmoothTool);
        wb.SetTerrainBrush(new TerrainBrushSettings(8f, 1f, 0.5f));
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.EndTerrainSculpt();
        Assert.InRange(field.GetHeight(53, 50), 99.9f, 100.1f);
        var after = field.Samples.ToArray();
        Assert.True(wb.CanUndoTerrain);
        wb.UndoTerrain();
        Assert.Equal(before, field.Samples.ToArray());
        wb.RedoTerrain();
        Assert.Equal(after, field.Samples.ToArray());
        wb.SelectTerrainSmoothTool();
        Assert.True(wb.IsTerrainSmoothTool);
        Assert.False(wb.IsTerrainFlattenTool);
        wb.SelectTerrainElevationTool(true);
        Assert.True(wb.IsTerrainElevationTool);
        Assert.False(wb.IsTerrainFlattenTool);
        Assert.Equal(999f, wb.TerrainTargetElevation);
        Assert.True(wb.TerrainElevationArea);
        wb.SelectTerrainFlattenTool();
        wb.SelectTerrainElevationTool(false);
        Assert.False(wb.IsTerrainFlattenTool);
        Assert.False(wb.IsTerrainElevationTool);
    }
}