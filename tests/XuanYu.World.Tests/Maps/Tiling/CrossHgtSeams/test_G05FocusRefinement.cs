using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05FocusRefinement
{
    [Fact]
    public void LimitedBudget_RefinesFocusAcrossHgtBoundary()
    {
        var focus = new GeoCoordinate(23.5d, 121d, 0d);
        var camera = new GlobalCamera(focus, 1000d);
        camera.FrameTop(focus, 1000d);
        var view = new GeoTileView(camera.PositionEcef, camera.TargetEcef,
            Math.PI / 3d, 1280, 720);
        var coverage = new GeoTileBounds(23d, 120d, 25d, 122d);
        var selector = new GeoTileSelector(200_000d, 4d, 13, 256);
        GeoTileId expected = GeographicTileScheme.Find(focus, 13);
        var ids = selector.Select(view, coverage, HasCoverage, focus);
        Assert.Contains(expected, ids);
        Assert.InRange(ids.Count, 1, 256);
        Assert.Equal(ids.Count, ids.Distinct().Count());
        var set = ids.ToHashSet();
        foreach (GeoTileId id in ids)
        {
            Assert.True(HasCoverage(id));
            for (GeoTileId? parent = GeographicTileScheme.Parent(id);
                parent is GeoTileId p; parent = GeographicTileScheme.Parent(p))
                Assert.DoesNotContain(p, set);
        }
    }

    private static bool HasCoverage(GeoTileId id)
    {
        GeoTileBounds b = GeographicTileScheme.Bounds(id);
        return b.SouthLatitudeDegrees < 24d || b.EastLongitudeDegrees > 121d;
    }
}