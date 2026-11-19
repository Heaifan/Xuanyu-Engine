using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05DemSelection
{
    [Fact]
    public void Selection_MarksOnlyRequestedHgtWithoutTintAndClearsOnReimport()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var wb = new Workbench();
        wb.ImportTerrainFiles([east.Path, west.Path]);
        Assert.Equal(new[] { "N24E120", "N24E121" }, wb.TerrainDemEntries.Select(e => e.TileId));
        Assert.Null(wb.SelectedTerrainDem);
        wb.ToggleGeoTerrainNative();
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121, 0));
        wb.SelectTerrainDem("N24E120");
        Assert.Equal(west.Path, wb.SelectedTerrainDem!.Path);
        CheckDraws(wb.GeoTerrainDraws(1280, 720), anchor, true);
        wb.SelectTerrainDem("N24E121");
        CheckDraws(wb.GeoTerrainDraws(1280, 720), anchor, false);
        wb.SelectTerrain(true);
        Assert.Null(wb.SelectedTerrainDem);
        Assert.All(wb.GeoTerrainDraws(1280, 720), draw => Assert.False(draw.Selected));
        wb.SelectTerrainDem("N24E120");
        wb.ImportTerrain(east.Path);
        Assert.Null(wb.SelectedTerrainDem);
        Assert.Empty(wb.TerrainDemEntries);
        Assert.All(wb.GeoTerrainDraws(1280, 720), draw => Assert.False(draw.Selected));
    }

    [Fact]
    public void NativeDetail_OnSelectedHgtUsesOutlineWithoutTint()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var wb = new Workbench();
        wb.ImportTerrainFiles([west.Path, east.Path]);
        var target = new GeoCoordinate(24.5, 121.5, 0);
        wb.GeoTerrainCamera!.FrameTop(target, 1000);
        wb.SelectTerrainDem("N24E121");
        GeoTileId id = GeographicTileScheme.Find(target, 13);
        RenderMesh? mesh = null;
        for (int i = 0; i < 150 && mesh is null; i++)
            mesh = wb.GeoTerrainDraws(1280, 720).Where(draw => draw.Selected)
                .Select(draw => draw.Mesh).FirstOrDefault(m =>
                    m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) &&
                    m.AssetId.Contains($"/{id}/", StringComparison.Ordinal));
        Assert.NotNull(mesh);
        CheckOriginalColor(mesh!);
        wb.SelectTerrain(true);
        Assert.All(wb.GeoTerrainDraws(1280, 720), draw => Assert.False(draw.Selected));
    }

    private static void CheckDraws(IReadOnlyList<RenderDraw> draws, EnuFrame anchor, bool west)
    {
        Assert.Contains(draws, draw => draw.Selected);
        Assert.Contains(draws, draw => !draw.Selected);
        foreach (var draw in draws)
        {
            CheckOriginalColor(draw.Mesh);
            float[] v = draw.Mesh.Vertices.ToArray();
            for (int i = 0; i < v.Length; i += 21)
            {
                Vector3 p = (Point(v, i) + Point(v, i + 7) + Point(v, i + 14)) / 3f;
                GeoCoordinate g = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(p.X, p.Y, p.Z)));
                if (Math.Abs(g.LongitudeDegrees - 121d) > 0.001d)
                    Assert.Equal(west == (g.LongitudeDegrees < 121d), draw.Selected);
            }
        }
    }

    private static void CheckOriginalColor(RenderMesh mesh)
    {
        float[] v = mesh.Vertices.ToArray();
        Vector3 expected = new(0x5C / 255f, 0xBB / 255f, 0x6F / 255f);
        for (int i = 0; i < v.Length; i += 7)
        {
            Assert.InRange(Vector3.Distance(Point(v, i + 3), expected), 0f, 0.0001f);
            Assert.Equal(1f, v[i + 6]);
        }
    }

    private static Vector3 Point(float[] data, int i) => new(data[i], data[i + 1], data[i + 2]);
}