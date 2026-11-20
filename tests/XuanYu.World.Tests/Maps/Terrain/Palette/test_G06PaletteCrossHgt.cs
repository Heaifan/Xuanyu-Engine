using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06PaletteCrossHgt
{
    [Fact]
    public void NativeMesh_CrossesHgtBoundaryWithAbsoluteColors()
    {
        using var west = new G02BHgtFixture("N23E120");
        using var east = new G02BHgtFixture("N23E121");
        west.SetHeight(3598, 1801, 800);
        east.SetHeight(2, 1801, 2800);
        NasademTile westTile = west.Load();
        var source = new NasademElevationSource(westTile, east.Load());
        var anchor = new EnuFrame(new GeoCoordinate(23.5, 121, 0));
        GeoCoordinate left = new(23d + 1801d / 3600d,
            120d + 3598d / 3600d, 0d);
        GeoCoordinate right = new(23d + 1801d / 3600d,
            121d + 2d / 3600d, 0d);
        GeoTileId id = GeographicTileScheme.Find(left, 13);
        Assert.Equal(id, GeographicTileScheme.Find(right, 13));
        RenderMesh mesh = GeoTerrainNativeMeshBuilder.Build(westTile, id,
            anchor, source, new GeoTileBounds(23, 120, 24, 122), default)!;
        AssertSample(mesh, anchor, left, 800d, 0xA8D96D);
        AssertSample(mesh, anchor, right, 2800d, 0xEDCF79);
    }

    private static void AssertSample(RenderMesh mesh, EnuFrame anchor,
        GeoCoordinate coordinate, double height, int rgb)
    {
        EnuPosition flat = anchor.ToEnu(Wgs84.ToEcef(coordinate));
        Vector3 target = new((float)flat.East, (float)flat.North,
            (float)(flat.Up + height));
        Vector3 expected = new(((rgb >> 16) & 255) / 255f,
            ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        for (int i = 0; i < data.Length; i += 7)
        {
            Vector3 point = new(data[i], data[i + 1], data[i + 2]);
            if (Vector3.Distance(point, target) >= 0.1f) continue;
            Vector3 color = new(data[i + 3], data[i + 4], data[i + 5]);
            Assert.InRange(Vector3.Distance(color, expected), 0f, 0.00002f);
            Assert.Equal(1f, data[i + 6]);
            Assert.Equal((float)height, mesh.TerrainElevations.Span[i / 7], 2);
            return;
        }
        Assert.Fail($"未找到 HGT 原生采样顶点：{coordinate}");
    }
}
