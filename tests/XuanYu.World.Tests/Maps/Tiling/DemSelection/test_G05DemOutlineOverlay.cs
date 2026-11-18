using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05DemOutlineOverlay
{
    [Fact]
    public void OnlySelectedDemEnablesOverlayAndClearRestoresNormalDraws()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var wb = new Workbench();
        wb.ImportTerrainFiles([west.Path, east.Path]);
        wb.ToggleGeoTerrainNative();
        wb.SelectTerrainDem("N24E120");
        var draws = wb.GeoTerrainDraws(1280, 720);
        Assert.Contains(draws, d => d.Selected && d.OutlineThroughOcclusion);
        Assert.Contains(draws, d => !d.Selected);
        Assert.All(draws, d => Assert.Equal(d.Selected, d.OutlineThroughOcclusion));
        var regular = new RenderDraw(draws[0].Mesh, Matrix4x4.Identity, true, true);
        Assert.False(regular.OutlineThroughOcclusion);
        wb.SelectTerrain(true);
        Assert.All(wb.GeoTerrainDraws(1280, 720), d =>
        {
            Assert.False(d.Selected);
            Assert.False(d.OutlineThroughOcclusion);
        });
    }
}