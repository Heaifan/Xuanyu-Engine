using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05MultiIrregular
{
    [Fact]
    public void ThreeHgt_KeepActualFootprintAcrossFrames()
    {
        using var a = new G02BHgtFixture("N23E120");
        using var b = new G02BHgtFixture("N23E121");
        using var c = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(a.Load(), b.Load(), c.Load());
        Assert.Equal(3, session.SourceCount);
        var anchor = new EnuFrame(new GeoCoordinate(24d, 121d, 0d));
        session.ToggleNative();
        CheckFrame(session, anchor);
        session.Camera.FrameTop(new GeoCoordinate(24.1d, 121.1d, 0d), 8000d);
        for (int i = 0; i < 24; i++) CheckFrame(session, anchor);
        session.Camera.FrameTop(new GeoCoordinate(23.5d, 120.5d, 0d), 8000d);
        for (int i = 0; i < 12; i++) CheckFrame(session, anchor);
    }

    private static void CheckFrame(GeoTerrainRenderSession session, EnuFrame anchor)
    {
        var draws = session.Draws(1280, 720);
        Assert.NotEmpty(draws);
        var seen = new HashSet<(int, int)>();
        foreach (var draw in draws)
        {
            float[] data = draw.Mesh.Vertices.ToArray();
            for (int i = 0; i < data.Length; i += 21)
            {
                var p = (Point(data, i) + Point(data, i + 7) + Point(data, i + 14)) / 3f;
                GeoCoordinate geo = Wgs84.ToGeo(anchor.ToEcef(new EnuPosition(p.X, p.Y, p.Z)));
                int lat = (int)Math.Floor(geo.LatitudeDegrees);
                int lon = (int)Math.Floor(geo.LongitudeDegrees);
                Assert.True((lat, lon) is (23, 120) or (23, 121) or (24, 121),
                    $"实际三角形落到未导入区域：{geo.LatitudeDegrees}, {geo.LongitudeDegrees}");
                seen.Add((lat, lon));
            }
        }
        Assert.Equal(3, seen.Count);
    }

    private static Vector3 Point(float[] data, int index) =>
        new(data[index], data[index + 1], data[index + 2]);
}