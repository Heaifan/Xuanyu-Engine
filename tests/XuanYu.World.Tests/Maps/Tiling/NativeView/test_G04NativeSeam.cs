using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04NativeSeam
{
    [Fact]
    public void NativeEastBoundary_IncludesAllCoarseWestKnots()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        NasademTile tile = hgt.Load();
        var source = new NasademElevationSource(tile);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        GeoTileId west = GeographicTileScheme.Find(
            new GeoCoordinate(24.5, 121.5, 0), 13);
        GeoTileId east = GeographicTileScheme.East(west);
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        RenderMesh native = GeoTerrainNativeMeshBuilder.Build(tile, west,
            anchor, source, coverage, default)!;
        RenderMesh coarse = GeoTerrainMeshBuilder.Build(
            GeoHeightTileBuilder.Build(source, east, 16),
            anchor, source, coverage)!;
        float[] a = native.Vertices.ToArray();
        float[] b = coarse.Vertices.ToArray();
        for (int row = 0; row <= 16; row++)
        {
            int vertex = row == 16 ? 6 * (15 * 16) + 1 : 6 * row * 16;
            Vector3 expected = Point(b, vertex);
            Assert.Contains(Enumerable.Range(0, native.VertexCount), i =>
                Vector3.Distance(Point(a, i), expected) < 0.1f);
        }
    }

    private static Vector3 Point(float[] data, int vertex)
    {
        int i = vertex * 7;
        return new Vector3(data[i], data[i + 1], data[i + 2]);
    }
}