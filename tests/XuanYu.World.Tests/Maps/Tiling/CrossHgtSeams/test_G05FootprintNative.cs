using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;
namespace XuanYu.World.Tests.Maps;

public sealed class test_G05FootprintNative
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IrregularSet_NativeRetainsRealSamplesOnBothSidesOfSourceBoundary(bool overview)
    {
        using var west = new G02BHgtFixture("N23E120");
        using var east = new G02BHgtFixture("N23E121");
        using var north = new G02BHgtFixture("N24E121");
        west.SetHeight(3598, 1801, 1300);
        east.SetHeight(2, 1801, 2300);
        var session = overview
            ? new GeoTerrainRenderSession(new NasademTileCatalog([west.Path, east.Path, north.Path]), 16, useOverview: true)
            : new GeoTerrainRenderSession(west.Load(), east.Load(), north.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24d, 121d, 0d));
        var left = new GeoCoordinate(23d + 1801d / 3600d, 120d + 3598d / 3600d, 0d);
        var right = new GeoCoordinate(23d + 1801d / 3600d, 121d + 2d / 3600d, 0d);
        var id = GeographicTileScheme.Find(left, 13);
        Assert.Equal(id, GeographicTileScheme.Find(right, 13));
        session.Camera.FrameTop(new GeoCoordinate(23.5d, 121d, 0d), 1000d);
        RenderMesh[] meshes = [];
        for (int i = 0; i < 150 && meshes.Length == 0; i++)
            meshes = session.Draws(1280, 720).Select(d => d.Mesh).Where(m =>
                m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) && m.AssetId.Contains($"/{id}/", StringComparison.Ordinal)).ToArray();
        Assert.NotEmpty(meshes);
        AssertSample(meshes, anchor, left, 1300d);
        AssertSample(meshes, anchor, right, 2300d);
    }
    [Fact]
    public void AdjacentNativeTiles_ShareRawRidgeOnTheirCommonEdge()
    {
        using var island = new G02BHgtFixture("N22E121");
        using var north = new G02BHgtFixture("N23E121");
        const int row = 273;
        double latitude = 22d + row / 3600d;
        var left = GeographicTileScheme.Find(new GeoCoordinate(latitude, 121.505d, 0d), 13);
        double longitude = GeographicTileScheme.Bounds(left).EastLongitudeDegrees;
        int column = (int)Math.Floor((longitude - 121d) * 3600d);
        island.SetHeight(column, row, 700);
        island.SetHeight(column + 1, row, 700);
        var session = new GeoTerrainRenderSession(new NasademTileCatalog([island.Path, north.Path]),
            16, useOverview: true);
        var point = new GeoCoordinate(latitude, longitude, 0d);
        session.Camera.FrameTop(point, 2000d);
        var right = new GeoTileId(13, left.X + 1, left.Y);
        RenderMesh[] pair = [];
        for (int frame = 0; frame < 180 && pair.Length < 2; frame++)
            pair = session.Draws(1280, 720).Select(d => d.Mesh).Where(m =>
                m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) &&
                (m.AssetId.Contains($"/{left}/") || m.AssetId.Contains($"/{right}/"))).ToArray();
        Assert.Equal(2, pair.Length);
        var anchor = new EnuFrame(new GeoCoordinate(23d, 121.5d, 0d));
        foreach (var mesh in pair) AssertSample([mesh], anchor, point, 700d);
    }
    [Fact]
    public void InheritedCoarseEdge_FirstNativeRowDoesNotFormSpur()
    {
        using var south = new G02BHgtFixture("N21E121");
        using var island = new G02BHgtFixture("N22E121");
        using var east = new G02BHgtFixture("N22E122");
        GeoTileId id = new(13, 13724, 3091);
        GeoTileBounds b = GeographicTileScheme.Bounds(id);
        int column = (int)Math.Round((b.WestLongitudeDegrees + b.EastLongitudeDegrees - 242d) * 1800d);
        island.SetHeight(column, 218, 100);
        var session = new GeoTerrainRenderSession(south.Load(), island.Load(), east.Load());
        session.Camera.FrameTop(new GeoCoordinate(b.SouthLatitudeDegrees + 0.001d,
            (b.WestLongitudeDegrees + b.EastLongitudeDegrees) / 2d, 0d), 2000d);
        RenderMesh? mesh = null;
        for (int i = 0; i < 180 && mesh is null; i++)
            mesh = session.Draws(1280, 720).Select(d => d.Mesh).FirstOrDefault(m =>
                m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) && m.AssetId.Contains($"/{id}/"));
        Assert.NotNull(mesh);
        double steepest = 0d;
        var v = mesh!.Vertices.Span;
        for (int i = 0; i + 20 < v.Length; i += 21)
            for (int e = 0; e < 3; e++)
            {
                int a = i + e * 7, c = i + ((e + 1) % 3) * 7;
                double dx = v[a] - v[c], dy = v[a + 1] - v[c + 1];
                steepest = Math.Max(steepest, Math.Abs(v[a + 2] - v[c + 2]) / Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.01d));
            }
        Assert.True(steepest < 5d, $"Inherited edge creates a {steepest:F1}:1 slope.");
    }
    private static void AssertSample(RenderMesh[] meshes, EnuFrame anchor, GeoCoordinate geo, double height)
    {
        EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(geo));
        Vector3 expected = new((float)p.East, (float)p.North, (float)(p.Up + height));
        Assert.Contains(meshes, mesh => Enumerable.Range(0, mesh.VertexCount).Any(i =>
            Vector3.Distance(new Vector3(mesh.Vertices.Span[i * 7], mesh.Vertices.Span[i * 7 + 1], mesh.Vertices.Span[i * 7 + 2]), expected) < 0.1f));
    }
}