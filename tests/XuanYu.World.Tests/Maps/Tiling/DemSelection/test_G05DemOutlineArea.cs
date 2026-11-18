using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05DemOutlineArea
{
    [Fact]
    public void FirstFrame_SelectionPreservesSurfaceAreaAcrossHgtBoundary()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var wb = new Workbench();
        wb.ImportTerrainFiles([west.Path, east.Path]);
        wb.ToggleGeoTerrainNative();
        double original = SurfaceArea(wb.GeoTerrainDraws(1280, 720));
        wb.ImportTerrainFiles([west.Path, east.Path]);
        wb.ToggleGeoTerrainNative();
        wb.SelectTerrainDem("N24E120");
        var draws = wb.GeoTerrainDraws(1280, 720);
        Assert.Contains(draws, draw => draw.Selected);
        Assert.Contains(draws, draw => !draw.Selected);
        Assert.True(original > 0d);
        Assert.InRange(SurfaceArea(draws) / original, 0.99999d, 1.00001d);
    }

    private static double SurfaceArea(IReadOnlyList<RenderDraw> draws)
    {
        double area = 0d;
        foreach (var draw in draws)
        {
            float[] v = draw.Mesh.Vertices.ToArray();
            for (int i = 0; i < v.Length; i += 21)
            {
                Vector3 a = Point(v, i), b = Point(v, i + 7), c = Point(v, i + 14);
                area += Vector3.Cross(b - a, c - a).Length() * 0.5d;
            }
        }
        return area;
    }

    private static Vector3 Point(float[] v, int i) => new(v[i], v[i + 1], v[i + 2]);
}