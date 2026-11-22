using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06DemPartition
{
    [Fact]
    public void Split_PreservesSurfaceAndAssignsFourHgtCells()
    {
        float[] vertices =
        [
            0, 0, 10, 1, 0, 0, 10, 0.8f, 0, 0,
            2, 0, 20, 0, 1, 0, 20, 0, 0.8f, 0,
            2, 2, 30, 0, 0, 1, 30, 0, 0, 0.8f,
            0, 0, 10, 1, 0, 0, 10, 0.8f, 0, 0,
            2, 2, 30, 0, 0, 1, 30, 0, 0, 0.8f,
            0, 2, 20, 0, 1, 0, 20, 0, 0.8f, 0
        ];
        double[] geo = [120.9, 23.9, 121.1, 23.9, 121.1, 24.1,
            120.9, 23.9, 121.1, 24.1, 120.9, 24.1];
        RenderMesh mesh = RenderMesh.FromLitTerrain("r05-test", vertices, geo);
        var anchor = new EnuFrame(new GeoCoordinate(24, 121, 0));
        var parts = GeoTerrainDemPartition.Split(mesh, anchor, 23, 120, 24, 121);
        Assert.Equal(4, parts.Length);
        Assert.Equal(4, parts.Select(p => (p.South, p.West)).Distinct().Count());
        double area = 0;
        foreach (var part in parts)
        {
            var data = part.Mesh.Vertices.Span;
            var coordinates = part.Mesh.TerrainGeographicCoordinates.Span;
            Assert.Equal(part.Mesh.VertexCount * 2, coordinates.Length);
            Assert.Equal(part.Mesh.VertexCount * 3, part.Mesh.TerrainShadedColors.Length);
            for (int i = 0; i < part.Mesh.VertexCount; i++)
            {
                Assert.InRange(coordinates[i * 2], part.West - 1e-7, part.West + 1d + 1e-7);
                Assert.InRange(coordinates[i * 2 + 1], part.South - 1e-7, part.South + 1d + 1e-7);
            }
            for (int i = 0; i < data.Length; i += 21)
            {
                Vector3 a = new(data[i], data[i + 1], data[i + 2]);
                Vector3 b = new(data[i + 7], data[i + 8], data[i + 9]);
                Vector3 c = new(data[i + 14], data[i + 15], data[i + 16]);
                area += Math.Abs(Vector3.Cross(b - a, c - a).Z) / 2d;
            }
        }
        Assert.InRange(area, 3.99999, 4.00001);
    }

    [Fact]
    public void NativeOff_VisibleBoundaryUsesBothImportedDemSourcesInHillshadeMode()
    {
        using var west = new G02BHgtFixture("N23E120");
        using var east = new G02BHgtFixture("N23E121");
        var session = new GeoTerrainRenderSession(new NasademTileCatalog([west.Path, east.Path]),
            4, useOverview: true, streamRaw: true);
        session.ToggleNative();
        bool enabled = Environment.GetEnvironmentVariable("XYE_G06_DEM_HILLSHADE") == "1";
        var target = new GeoCoordinate(23.5, 121.0002, 0);
        var id = GeographicTileScheme.Find(target, 13);
        Assert.Equal(id, GeographicTileScheme.Find(new GeoCoordinate(23.5, 120.9998, 0), 13));
        session.Camera.FrameTop(target, 1000d);
        RenderDraw[] parts = [];
        Assert.True(SpinWait.SpinUntil(() =>
        {
            parts = session.Draws(1280, 720).Where(d =>
                d.Mesh.AssetId == $"terrain/g02/{id}" ||
                d.Mesh.AssetId.StartsWith($"terrain/g02/{id}/", StringComparison.Ordinal)).ToArray();
            if (parts.Length == 0 || session.RawPrefetchPending) return false;
            return !enabled || parts.Any(d => d.ResidentDem?.WestLongitude == 120) &&
                parts.Any(d => d.ResidentDem?.WestLongitude == 121);
        }, TimeSpan.FromSeconds(30)), "跨 HGT 可见网格及原始数据未准备完成");
        Assert.Equal(0, session.NativeDrawCount);
        Assert.Equal(enabled ? 2 : 1, parts.Length);
        Assert.All(parts, p => Assert.False(p.LegacyContourOnly));
        if (enabled)
        {
            Assert.All(parts, p => Assert.True(p.ResidentDem.HasValue && !p.DemUV.IsEmpty));
            Assert.Equal(new[] { 120, 121 }, parts.Select(p => p.ResidentDem!.Value.WestLongitude).Order().ToArray());
        }
    }
}