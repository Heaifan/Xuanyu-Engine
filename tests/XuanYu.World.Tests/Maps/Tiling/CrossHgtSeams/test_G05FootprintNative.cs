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
            ? new GeoTerrainRenderSession(new NasademTileCatalog([west.Path, east.Path, north.Path]),
                16, useOverview: true)
            : new GeoTerrainRenderSession(west.Load(), east.Load(), north.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24d, 121d, 0d));
        var left = new GeoCoordinate(23d + 1801d / 3600d, 120d + 3598d / 3600d, 0d);
        var right = new GeoCoordinate(23d + 1801d / 3600d, 121d + 2d / 3600d, 0d);
        var id = GeographicTileScheme.Find(left, 13);
        Assert.Equal(id, GeographicTileScheme.Find(right, 13));
        session.Camera.FrameTop(new GeoCoordinate(23.5d, 121d, 0d), 1000d);
        RenderMesh? mesh = null;
        for (int i = 0; i < 150 && mesh is null; i++)
            mesh = session.Draws(1280, 720).Select(d => d.Mesh).FirstOrDefault(m =>
                m.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal) &&
                m.AssetId.Contains($"/{id}/", StringComparison.Ordinal));
        Assert.NotNull(mesh);
        AssertSample(mesh!, anchor, left, 1300d);
        AssertSample(mesh!, anchor, right, 2300d);
    }

    private static void AssertSample(RenderMesh mesh, EnuFrame anchor, GeoCoordinate geo, double height)
    {
        EnuPosition flat = anchor.ToEnu(Wgs84.ToEcef(geo));
        Vector3 expected = new((float)flat.East, (float)flat.North, (float)(flat.Up + height));
        float[] v = mesh.Vertices.ToArray();
        Assert.Contains(Enumerable.Range(0, mesh.VertexCount), i =>
            Vector3.Distance(new Vector3(v[i * 7], v[i * 7 + 1], v[i * 7 + 2]), expected) < 0.1f);
    }
}