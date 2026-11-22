using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05StreamingViewport
{
    [Fact]
    public void FarNavigation_UsesOverviewWithoutRawDecodes()
    {
        using var a = new G02BHgtFixture("N24E120");
        using var b = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(new NasademTileCatalog([a.Path, b.Path]),
            4, useOverview: true, streamRaw: true);
        Assert.True(session.SourceResidency.LoadCount == 0,
            $"构造阶段已读取原始 HGT：{session.SourceResidency.LoadCount}");
        foreach (double lon in new[] { 120.5, 121.5, 120.5 })
        {
            session.Camera.FrameTop(new GeoCoordinate(24.5, lon, 0), 150_000d);
            var eye = Wgs84.ToGeo(session.Camera.PositionEcef);
            Assert.True(session.SourceResidency.LoadCount == 0,
                $"FrameTop 读取原始 HGT：经度 {lon}，眼点高程 {eye.HeightMeters:F1}m，" +
                $"解码 {session.SourceResidency.LoadCount}");
            for (int i = 0; i < 6; i++)
            {
                Assert.NotEmpty(session.Draws(1000, 700));
                Assert.True(session.NativeDrawCount == 0,
                    $"远景意外触发 Native：经度 {lon}，帧 {i}");
                Assert.True(session.SourceResidency.LoadCount == 0,
                    $"Draws 触发原始 HGT：经度 {lon}，帧 {i}，" +
                    $"眼点高程 {eye.HeightMeters:F1}m，解码 {session.SourceResidency.LoadCount}，" +
                    $"后台完成 {session.BackgroundLoadCount}，在途 {session.RawPrefetchPending}");
            }
        }
        Assert.False(session.RawPrefetchPending);
    }

    [Fact]
    public void NativeBoundary_KeepsCoverageUntilRawIsAvailable()
    {
        using var a = new G02BHgtFixture("N23E120");
        using var b = new G02BHgtFixture("N23E121");
        using var c = new G02BHgtFixture("N24E121");
        a.SetHeight(3598, 1801, 1300);
        b.SetHeight(2, 1801, 2300);
        var session = new GeoTerrainRenderSession(new NasademTileCatalog([a.Path, b.Path, c.Path]),
            4, useOverview: true, streamRaw: true);
        var target = new GeoCoordinate(23.5, 121, 0);
        session.Camera.FrameTop(target, 1000d);
        var id = GeographicTileScheme.Find(target, 13);
        var anchor = new EnuFrame(new GeoCoordinate(24d, 121d, 0d));
        XuanYu.Render.RenderMesh[] meshes = [];
        Assert.True(SpinWait.SpinUntil(() =>
        {
            var draws = session.Draws(1280, 720);
            Assert.NotEmpty(draws);
            meshes = draws.Select(d => d.Mesh).Where(m =>
                m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) &&
                m.AssetId.Contains($"/{id}/", StringComparison.Ordinal)).ToArray();
            return meshes.Length > 0;
        }, TimeSpan.FromSeconds(30)));
        Assert.NotEmpty(meshes);
        Assert.True(session.BackgroundLoadCount > 0);
        foreach (var (lon, h) in new[] { (120d + 3598d / 3600d, 1300d),
            (121d + 2d / 3600d, 2300d) })
        {
            var p = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(23d + 1801d / 3600d, lon, 0)));
            var expected = new System.Numerics.Vector3((float)p.East, (float)p.North, (float)(p.Up + h));
            Assert.Contains(meshes, mesh => ContainsSample(mesh, expected));
        }
    }

    private static bool ContainsSample(XuanYu.Render.RenderMesh mesh, System.Numerics.Vector3 expected)
    {
        var v = mesh.Vertices.Span;
        for (int i = 0; i < v.Length; i += 7)
            if (System.Numerics.Vector3.Distance(new(v[i], v[i + 1], v[i + 2]), expected) < 0.1f)
                return true;
        return false;
    }
}