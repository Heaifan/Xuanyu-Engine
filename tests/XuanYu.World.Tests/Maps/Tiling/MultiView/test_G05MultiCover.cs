using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05MultiCover
{
    [Theory]
    [InlineData("N24E120", "N24E121", 24d, 120d, 25d, 122d)]
    [InlineData("N23E121", "N24E121", 23d, 121d, 25d, 122d)]
    [InlineData("N24W001", "N24E000", 24d, -1d, 25d, 1d)]
    public void AdjacentHgt_KeepFullCoverThroughNavigation(string a, string b,
        double south, double west, double north, double east)
    {
        using var first = new G02BHgtFixture(a);
        using var second = new G02BHgtFixture(b);
        var session = new GeoTerrainRenderSession(first.Load(), second.Load());
        var region = new GeoTileBounds(south, west, north, east);
        Assert.Equal(region, session.Coverage);
        session.ToggleNative();
        CheckFrame(session, region);
        session.Camera.FrameTop(new GeoCoordinate(south + 0.25, west + 0.25, 0), 8000);
        for (int i = 0; i < 24; i++) CheckFrame(session, region);
        session.Camera.FrameTop(new GeoCoordinate(north - 0.25, east - 0.25, 0), 8000);
        for (int i = 0; i < 24; i++) CheckFrame(session, region);
        for (int i = 0; i < 8; i++)
        {
            session.Navigate(90, -55, 0, false, 720);
            CheckFrame(session, region);
        }
        session.ResetCamera();
        Assert.Equal(300_000d, session.Camera.DistanceMeters);
        CheckFrame(session, region);
    }

    private static void CheckFrame(GeoTerrainRenderSession session, GeoTileBounds region)
    {
        var draws = session.Draws(1280, 720);
        var ids = session.VisibleTileIds.ToHashSet();
        Assert.NotEmpty(draws);
        Assert.Equal(ids.Count, draws.Count);
        Assert.All(draws, draw => Assert.True(draw.Mesh.TriangleCount > 0));
        double area = 0d;
        foreach (GeoTileId id in ids)
        {
            GeoTileId? parent = GeographicTileScheme.Parent(id);
            while (parent is GeoTileId ancestor)
            {
                Assert.DoesNotContain(ancestor, ids);
                parent = GeographicTileScheme.Parent(ancestor);
            }
            GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
            double lat = Math.Min(bounds.NorthLatitudeDegrees, region.NorthLatitudeDegrees) -
                Math.Max(bounds.SouthLatitudeDegrees, region.SouthLatitudeDegrees);
            double lon = Math.Min(bounds.EastLongitudeDegrees, region.EastLongitudeDegrees) -
                Math.Max(bounds.WestLongitudeDegrees, region.WestLongitudeDegrees);
            Assert.True(lat > 0 && lon > 0);
            area += lat * lon;
            Assert.Contains(draws, draw => draw.Mesh.AssetId == $"terrain/g02/{id}");
        }
        Assert.InRange(area, 2d - 1e-9d, 2d + 1e-9d);
    }
}