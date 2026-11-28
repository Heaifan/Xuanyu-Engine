using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.ViewportReference;

public sealed class test_E502AReferenceGrid
{
    [Theory]
    [InlineData(60d, 10d)]
    [InlineData(600d, 100d)]
    [InlineData(6000d, 1000d)]
    public void GridStep_ScalesWithDistance(double distance, double expected)
    {
        Assert.Equal(expected, GridStep.FromDistance(distance));
    }

    [Fact]
    public void MetricGrid_ReusesMeshUntilCameraMovesToAnotherRegion()
    {
        var camera = new ViewportCamera();
        var grid = new MetricGridOverlay();
        Matrix4x4 projection = camera.ViewProjection(800, 600);
        var first = grid.Draw(camera, projection);
        Assert.Equal("editor/metric-grid", first.Mesh.AssetId);
        Assert.Same(first.Mesh, grid.Draw(camera, projection).Mesh);
        camera.Pan(800f, 0f, 600);
        var next = grid.Draw(camera, camera.ViewProjection(800, 600));
        Assert.NotSame(first.Mesh, next.Mesh);
    }

    [Fact]
    public void EditorRetainsGridWithoutOpaqueReferenceGround()
    {
        var wb = new Workbench(use3D: true);
        var draws = wb.ViewportDraws(800, 600);
        Assert.Contains(draws, d => d.Mesh.AssetId == "editor/metric-grid");
        Assert.DoesNotContain(draws,
            d => d.Mesh.AssetId == "editor/reference-ground");
        wb.SetWorldGridEnabled(false);
        Assert.DoesNotContain(wb.ViewportDraws(800, 600),
            d => d.Mesh.AssetId == "editor/metric-grid");
    }

    [Fact]
    public void GeographicGrid_UsesMultipleTerrainFollowingSegments()
    {
        using var file = new XuanYu.World.Tests.Maps.G02BHgtFixture("N24E121");
        var terrain = new GeoTerrainRenderSession(file.Load());
        terrain.Camera.FrameTop(new GeoCoordinate(24.5d, 121.5d, 0d), 5000d);
        var grid = terrain.GridDraw(true, 800, 600);
        Assert.True(grid.HasValue);
        Assert.Equal("editor/geo-grid", grid.Value.Mesh.AssetId);
        Assert.True(grid.Value.Mesh.VertexCount > 20000);
    }
}