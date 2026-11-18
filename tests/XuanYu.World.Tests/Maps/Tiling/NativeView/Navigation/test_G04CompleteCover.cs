using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04CompleteCover
{
    [Theory]
    [InlineData("N23E121", 23d)]
    [InlineData("N24E121", 24d)]
    public void FirstFrameAndChangingView_KeepWholeHgtCovered(
        string tileId, double south)
    {
        using var hgt = new G02BHgtFixture(tileId);
        var session = new GeoTerrainRenderSession(hgt.Load());
        var region = new GeoTileBounds(south, 121d, south + 1d, 122d);
        for (int i = 0; i < 16; i++)
            CheckFrame(session, region);

        session.Camera.FrameTop(new GeoCoordinate(south + 0.25d, 121.25d, 0d), 8000d);
        for (int i = 0; i < 24; i++)
            CheckFrame(session, region);
        for (int i = 0; i < 12; i++)
        {
            session.Navigate(90, -55, 0, false, 720);
            CheckFrame(session, region);
        }
        session.Camera.FrameTop(new GeoCoordinate(south + 0.8d, 121.8d, 0d), 8000d);
        for (int i = 0; i < 24; i++)
            CheckFrame(session, region);
        session.Camera.Zoom(-200);
        CheckFrame(session, region);
    }

    private static void CheckFrame(GeoTerrainRenderSession session, GeoTileBounds region)
    {
        IReadOnlyList<RenderDraw> draws = session.Draws(1280, 720);
        var ids = session.VisibleTileIds.ToHashSet();
        Assert.NotEmpty(draws);
        Assert.Equal(ids.Count, draws.Count);
        Assert.All(draws, draw =>
        {
            Assert.True(draw.Visible);
            Assert.True(draw.Mesh.TriangleCount > 0);
        });
        double area = 0d;
        foreach (GeoTileId id in ids)
        {
            GeoTileId? parent = GeographicTileScheme.Parent(id);
            while (parent is GeoTileId ancestor)
            {
                Assert.DoesNotContain(ancestor, ids);
                parent = GeographicTileScheme.Parent(ancestor);
            }
            GeoTileBounds b = GeographicTileScheme.Bounds(id);
            double lat = Math.Min(b.NorthLatitudeDegrees, region.NorthLatitudeDegrees) -
                Math.Max(b.SouthLatitudeDegrees, region.SouthLatitudeDegrees);
            double lon = Math.Min(b.EastLongitudeDegrees, region.EastLongitudeDegrees) -
                Math.Max(b.WestLongitudeDegrees, region.WestLongitudeDegrees);
            Assert.True(lat > 0d && lon > 0d);
            area += lat * lon;
            Assert.Contains(draws, draw => draw.Mesh.AssetId == $"terrain/g02/{id}" ||
                draw.Mesh.AssetId.Contains($"/{id}/", StringComparison.Ordinal));
        }
        Assert.InRange(area, 1d - 1e-9d, 1d + 1e-9d);
    }
}