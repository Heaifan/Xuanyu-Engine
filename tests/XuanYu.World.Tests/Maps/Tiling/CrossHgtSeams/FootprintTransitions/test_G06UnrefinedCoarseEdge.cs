using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06UnrefinedCoarseEdge
{
    [Fact]
    public void FirstNativeRow_BlendsToUnrefinedCoarseEdge()
    {
        using var south = new G02BHgtFixture("N21E121");
        using var island = new G02BHgtFixture("N22E121");
        using var east = new G02BHgtFixture("N22E122");
        GeoTileId id = new(13, 13724, 3091);
        GeoTileBounds b = GeographicTileScheme.Bounds(id);
        int row = (int)Math.Floor((b.NorthLatitudeDegrees - 22d) * 3600d);
        int column = (int)Math.Round((b.WestLongitudeDegrees + b.EastLongitudeDegrees - 242d) * 1800d);
        for (int x = column - 2; x <= column + 2; x++) island.SetHeight(x, row, 100);
        var catalog = new NasademTileCatalog([south.Path, island.Path, east.Path]);
        var session = new GeoTerrainRenderSession(catalog, 16, useOverview: true);
        session.Camera.FrameTop(new GeoCoordinate(b.NorthLatitudeDegrees - 0.001d,
            (b.WestLongitudeDegrees + b.EastLongitudeDegrees) / 2d, 0d), 2000d);
        RenderMesh? mesh = null;
        for (int i = 0; i < 180 && mesh is null; i++)
            mesh = session.Draws(1280, 720).Select(d => d.Mesh).FirstOrDefault(m =>
                m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) && m.AssetId.Contains($"/{id}/"));
        Assert.NotNull(mesh);
        Assert.True(MaxSlope(mesh!) < 5d, "An unrefined coarse edge creates a terrain spur.");
    }

    private static double MaxSlope(RenderMesh mesh)
    {
        double max = 0d;
        var v = mesh.Vertices.Span;
        for (int i = 0; i + 20 < v.Length; i += 21)
            for (int e = 0; e < 3; e++)
            {
                int a = i + e * 7, c = i + ((e + 1) % 3) * 7;
                double dx = v[a] - v[c], dy = v[a + 1] - v[c + 1];
                max = Math.Max(max, Math.Abs(v[a + 2] - v[c + 2]) / Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.01d));
            }
        return max;
    }
}
