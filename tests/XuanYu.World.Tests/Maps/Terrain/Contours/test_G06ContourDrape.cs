using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06ContourDrape
{
    [Fact]
    public void ContourOpacity_FadesAcrossCoarseLods()
    {
        var style = new GeoContourStyle(500, 0.22f, 8f, 1f);
        Assert.Equal(0f, style.OpacityAtLod(7));
        Assert.Equal(0.11f, style.OpacityAtLod(8));
        Assert.Equal(0.22f, style.OpacityAtLod(9));
        Assert.Equal(0.22f, style.OpacityAtLod(10));
    }

    [Fact]
    public void SurfaceIndex_InterpolatesVisibleTriangles()
    {
        var surface = new GeoContourSurface(Plane());
        Assert.InRange(surface.HeightAt(25f, 35f)!.Value,
            204.24f, 204.26f);
        Assert.Null(surface.HeightAt(300f, 0f));
    }

    [Fact]
    public void RawContours_AreDrapedOverTheVisibleLodSurface()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        hgt.SetHeight(1800, 1800, 1000);
        var source = new NasademElevationSource(hgt.Load());
        var center = new GeoCoordinate(24.5d, 121.5d, 0d);
        var task = new GeoContourElevationTask(
            GeographicTileScheme.Find(center, 13),
            new GeoTileBounds(24d, 121d, 25d, 122d), source,
            new EnuFrame(center), new GeoContourStyle(100, 0.3f, 2f, 1f), 1);
        while (!task.Advance(8)) { }
        RenderMesh? lines = task.Finish(Plane());
        Assert.NotNull(lines);
        var v = lines.Vertices.Span;
        for (int i = 0; i < v.Length; i += 7)
            Assert.InRange(v[i + 2] - (200f + 0.1f * v[i] + 0.05f * v[i + 1]),
                0.98f, 1.02f);
    }

    [Fact]
    public void FirstTerrainFrame_CarriesContourSettingsOnTheSurfaceDraw()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var first = session.Draws(640, 480);
        Assert.NotEmpty(first);
        Assert.All(first, draw => Assert.True(draw.ContourIntervalMeters > 0));
        session.SetContoursEnabled(false);
        Assert.All(session.Draws(640, 480), draw => Assert.Equal(0f, draw.ContourIntervalMeters));
    }

    [Fact]
    public void CoarseFallbackMesh_DoesNotDrawUnsupportedContourDetail()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        var coarse = session.Draws(640, 480).Where(draw =>
        {
            const string prefix = "terrain/g02/";
            return draw.Mesh.AssetId.StartsWith(prefix, StringComparison.Ordinal) &&
                int.Parse(draw.Mesh.AssetId[prefix.Length..].Split('/')[0]) <= 7;
        }).ToArray();
        Assert.NotEmpty(coarse);
        Assert.All(coarse, draw => Assert.Equal(0f, draw.ContourOpacity));
    }

    private static RenderMesh Plane()
    {
        var data = new List<float>();
        Add(data, -200f, -200f); Add(data, 200f, -200f);
        Add(data, 200f, 200f); Add(data, -200f, -200f);
        Add(data, 200f, 200f); Add(data, -200f, 200f);
        return new RenderMesh("test/contour-surface", data.ToArray());
    }

    private static void Add(List<float> data, float x, float y)
    {
        data.AddRange([x, y, 200f + 0.1f * x + 0.05f * y, 1f, 1f, 1f, 1f]);
    }
}
