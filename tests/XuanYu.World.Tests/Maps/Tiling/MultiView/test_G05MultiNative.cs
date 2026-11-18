using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05MultiNative
{
    [Fact]
    public void NativeTileAcrossHgtBoundary_RetainsSamplesFromBothFiles()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        west.SetHeight(3598, 1801, 1300);
        east.SetHeight(2, 1801, 2300);
        NasademTile a = west.Load(), b = east.Load();
        var source = new NasademElevationSource(a, b);
        var region = new GeoTileBounds(24d, 120d, 25d, 122d);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121d, 0));
        var left = new GeoCoordinate(24 + 1801d / 3600d, 120 + 3598d / 3600d, 0);
        var right = new GeoCoordinate(24 + 1801d / 3600d, 121 + 2d / 3600d, 0);
        GeoTileId id = GeographicTileScheme.Find(left, 13);
        Assert.Equal(id, GeographicTileScheme.Find(right, 13));
        RenderMesh mesh = GeoTerrainNativeMeshBuilder.Build(a, id, anchor,
            source, region, default)!;
        AssertSample(mesh, anchor, left, 1300d);
        AssertSample(mesh, anchor, right, 2300d);
        Assert.True(mesh.TriangleCount > 512);
    }

    [Fact]
    public void Session_UsesNativeGridOnSecondHgt()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(west.Load(), east.Load());
        var target = new GeoCoordinate(24.5, 121.5, 0);
        session.Camera.FrameTop(target, 1000);
        GeoTileId id = GeographicTileScheme.Find(target, 13);
        bool found = false;
        for (int i = 0; i < 150 && !found; i++)
            found = session.Draws(1280, 720).Any(draw =>
                draw.Mesh.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) &&
                draw.Mesh.AssetId.Contains($"/{id}/", StringComparison.Ordinal));
        Assert.True(found, "第二张 HGT 的目标瓦片必须实际进入 Native 绘制。");
        Assert.True(session.NativeDrawCount > 0);
    }

    private static void AssertSample(RenderMesh mesh, EnuFrame anchor,
        GeoCoordinate geo, double elevation)
    {
        EnuPosition flat = anchor.ToEnu(Wgs84.ToEcef(geo));
        var expected = new Vector3((float)flat.East, (float)flat.North,
            (float)(flat.Up + elevation));
        float[] data = mesh.Vertices.ToArray();
        Assert.Contains(Enumerable.Range(0, mesh.VertexCount), i =>
            Vector3.Distance(new Vector3(data[i * 7], data[i * 7 + 1],
                data[i * 7 + 2]), expected) < 0.1f);
    }
}