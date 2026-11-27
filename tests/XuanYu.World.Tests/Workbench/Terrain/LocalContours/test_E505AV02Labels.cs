using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E505AV02Labels
{
    [Fact]
    public void FlatSurface_DoesNotInventLabels()
    {
        var (draw, geometry) = Plane(false);
        Assert.Empty(LocalContourLabelCandidates.Build(draw, geometry, 2, 800, 600));
    }

    [Fact]
    public void Slope_LabelsRealMajorElevationsAndConnectsSharedEdges()
    {
        var (draw, geometry) = Plane(true);
        var labels = LocalContourLabelCandidates.Build(draw, geometry, 2, 800, 600);
        Assert.NotEmpty(labels);
        Assert.All(labels, label =>
        {
            Assert.Equal(0, label.ElevationMeters % 10);
            Assert.Equal((float)label.ElevationMeters, label.Up);
            Assert.Equal((float)label.ElevationMeters, label.North * 0.3f, 3);
            Assert.Same(draw.Mesh, label.Owner);
            Assert.True(label.ContourId > 0);
        });
        foreach (var group in labels.GroupBy(label => label.ElevationMeters))
            Assert.Single(group.Select(label => label.ContourId).Distinct());
    }

    [Fact]
    public void Workbench_LabelToggleAndActiveStrokeSuppressLocalLabels()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100, 100, 1);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        Assert.True(wb.TerrainContourLabelsEnabled);
        wb.SetTerrainContourLabelsEnabled(false);
        Assert.False(wb.TerrainContourLabelsEnabled);
        wb.SetTerrainContourLabelsEnabled(true);
        wb.SelectTerrainElevationTool(true);
        wb.SetTerrainElevation(100, false);
        Assert.True(wb.TryBeginTerrainSculpt(new(400, 300), 800, 600, false));
        var draws = wb.ApplyLocalTerrainContours(wb.ViewportDraws(800, 600));
        Assert.Empty(wb.BindTerrainContourLabels(draws, [], 800, 600));
        wb.EndTerrainSculpt();
        wb.SetTerrainContoursEnabled(false);
        draws = wb.ApplyLocalTerrainContours(wb.ViewportDraws(800, 600));
        Assert.Empty(wb.BindTerrainContourLabels(draws, [], 800, 600));
    }

    private static (RenderDraw, TerrainMeshGeometry) Plane(bool slope)
    {
        var field = new TerrainHeightField(11, 11, 10);
        if (slope)
            for (int y = 0; y < 11; y++)
                for (int x = 0; x < 11; x++) field.SetHeight(x, y, y * 3);
        var geometry = TerrainMeshBuilder.Build(field);
        var mesh = RenderMesh.FromLocalTerrain("test/local-labels",
            TerrainRenderMeshAdapter.Create(geometry).Vertices.Span);
        var projection = Matrix4x4.CreateScale(0.01f, 0.01f, 0.001f) *
            Matrix4x4.CreateTranslation(-0.5f, -0.5f, 0.2f);
        return (new RenderDraw(mesh, projection, true, false), geometry);
    }
}