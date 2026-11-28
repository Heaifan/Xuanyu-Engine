using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests;

public sealed class test_E505AV02Contours
{
    [Theory]
    [InlineData(50d, 1)]
    [InlineData(200d, 2)]
    [InlineData(500d, 5)]
    [InlineData(1000d, 10)]
    [InlineData(2000d, 20)]
    [InlineData(5000d, 50)]
    public void LocalDistance_SelectsMeterIntervals(double distance, int interval)
    {
        var style = GeoContourStyle.AtLocalDistance(distance);
        Assert.Equal(interval, style.IntervalMeters);
        Assert.Equal(interval, style.NextIntervalMeters);
    }

    [Fact]
    public void LocalTransition_BorrowsG06Blend()
    {
        var style = GeoContourStyle.AtLocalDistance(100d);
        Assert.Equal(1, style.IntervalMeters);
        Assert.Equal(2, style.NextIntervalMeters);
        Assert.Equal(0.5f, style.IntervalBlend, 4);
        Assert.Equal(GeoContourStyle.AtDistance(2500d),
            GeoContourStyle.AtLocalDistance(2500d));
    }

    [Fact]
    public void LocalMesh_HeightChannelSurvivesSculptUndoAndRedo()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(8848, false);
        var obj = Assert.Single(wb.Scene.Objects);
        CheckChannel(obj.Mesh!, 0f);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        wb.AdvanceTerrainSculpt(1);
        wb.EndTerrainSculpt();
        CheckChannel(obj.Mesh!, 8848f);
        wb.UndoTerrain();
        CheckChannel(obj.Mesh!, 0f);
        wb.RedoTerrain();
        CheckChannel(obj.Mesh!, 8848f);
    }

    [Fact]
    public void Toggle_ChangesOnlyTerrainDrawParameters()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        var mesh = Assert.Single(wb.Scene.Objects).Mesh;
        var source = wb.ViewportDraws(800, 600);
        var enabled = wb.ApplyLocalTerrainContours(source);
        var active = Assert.Single(enabled, d => ReferenceEquals(d.Mesh, mesh));
        Assert.True(active.ContourIntervalMeters > 0);
        Assert.True(active.ContourOpacity > 0);
        wb.SetTerrainContoursEnabled(false);
        var disabled = wb.ApplyLocalTerrainContours(source);
        Assert.Equal(0f, Assert.Single(disabled, d => ReferenceEquals(d.Mesh, mesh)).ContourOpacity);
        Assert.Equal(source.Where(d => !ReferenceEquals(d.Mesh, mesh)).ToArray(),
            enabled.Where(d => !ReferenceEquals(d.Mesh, mesh)).ToArray());
        Assert.Same(mesh, Assert.Single(wb.Scene.Objects).Mesh);
    }

    private static void CheckChannel(RenderMesh mesh, float max)
    {
        var heights = mesh.TerrainElevations.Span;
        var vertices = mesh.Vertices.Span;
        Assert.Equal(mesh.VertexCount, heights.Length);
        Assert.Equal(max, heights.ToArray().Max());
        for (int i = 0; i < heights.Length; i++)
        {
            Assert.Equal(vertices[i * 7 + 2], heights[i]);
            Assert.Equal(1f, vertices[i * 7 + 6]);
        }
    }
}