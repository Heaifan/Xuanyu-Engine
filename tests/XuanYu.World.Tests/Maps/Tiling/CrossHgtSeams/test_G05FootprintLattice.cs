using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05FootprintLattice
{
    [Fact]
    public void IrregularSet_InteriorTriangleActuallyCrossesOriginalHgtBoundary()
    {
        using var west = new G02BHgtFixture("N23E120");
        using var east = new G02BHgtFixture("N23E121");
        using var north = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(west.Load(), east.Load(), north.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24d, 121d, 0d));
        session.ToggleNative();
        session.Camera.FrameTop(new GeoCoordinate(23.5d, 121d, 0d), 8000d);
        bool crossed = false;
        for (int i = 0; i < 120 && !crossed; i++)
        {
            var draws = session.Draws(1280, 720);
            crossed = draws.Any(draw => CrossesInterior(draw.Mesh, anchor));
        }
        Assert.True(crossed, "实际内部三角形需要跨越 HGT 接口，而非沿源文件边界分别剖分。");
    }

    private static bool CrossesInterior(RenderMesh mesh, EnuFrame anchor)
    {
        ReadOnlySpan<float> v = mesh.Vertices.Span;
        for (int i = 0; i < v.Length; i += 21)
        {
            GeoCoordinate a = Read(v, i, anchor), b = Read(v, i + 7, anchor), c = Read(v, i + 14, anchor);
            double south = Math.Min(a.LatitudeDegrees, Math.Min(b.LatitudeDegrees, c.LatitudeDegrees));
            double north = Math.Max(a.LatitudeDegrees, Math.Max(b.LatitudeDegrees, c.LatitudeDegrees));
            if (south < 23.2d || north > 23.8d) continue;
            double west = Math.Min(a.LongitudeDegrees, Math.Min(b.LongitudeDegrees, c.LongitudeDegrees));
            double east = Math.Max(a.LongitudeDegrees, Math.Max(b.LongitudeDegrees, c.LongitudeDegrees));
            if (west < 121d - 1e-6d && east > 121d + 1e-6d) return true;
        }
        return false;
    }

    private static GeoCoordinate Read(ReadOnlySpan<float> v, int i, EnuFrame anchor) =>
        Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(v[i], v[i + 1], v[i + 2])));
}