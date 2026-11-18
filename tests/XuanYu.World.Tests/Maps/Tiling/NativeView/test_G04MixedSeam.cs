using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04MixedSeam
{
    [Fact]
    public void NativeFineWestBorder_ContainsCoarseEastKnots()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        NasademTile tile = hgt.Load();
        var source = new NasademElevationSource(tile);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        GeoTileId coarseId = GeographicTileScheme.Find(
            new GeoCoordinate(24.5, 121.5, 0), 12);
        GeoTileId fineId = GeographicTileScheme.Children(
            GeographicTileScheme.East(coarseId))[0];
        RenderMesh coarse = GeoTerrainMeshBuilder.Build(
            GeoHeightTileBuilder.Build(source, coarseId, 16),
            anchor, source, coverage)!;
        var neighbors = new GeoTerrainEdgeNeighbors(null, null, coarseId, null);
        RenderMesh fine = GeoTerrainNativeMeshBuilder.Build(tile, fineId,
            anchor, source, coverage, neighbors)!;
        float[] a = coarse.Vertices.ToArray();
        float[] b = fine.Vertices.ToArray();
        for (int row = 0; row <= 8; row++)
        {
            int index = 6 * (row * 16 + 15) + 2;
            Vector3 expected = Point(a, index);
            Assert.Contains(Enumerable.Range(0, fine.VertexCount), i =>
                Vector3.Distance(Point(b, i), expected) < 0.1f);
        }
    }

    private static Vector3 Point(float[] data, int vertex)
    {
        int i = vertex * 7;
        return new Vector3(data[i], data[i + 1], data[i + 2]);
    }
}