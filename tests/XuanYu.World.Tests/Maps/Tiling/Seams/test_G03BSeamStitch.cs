using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G03BSeamStitch
{
    [Fact]
    public void FineWestEdge_AlignsToCoarseEastSegments()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        foreach (int row in new[] { 1799, 1800 })
            foreach (int col in new[] { 2306, 2307 })
                hgt.SetHeight(col, row, 2000);
        var source = new NasademElevationSource(hgt.Load());
        GeoTileId coarseId = new(8, 428, 93);
        GeoTileId fineId = GeographicTileScheme.Children(
            GeographicTileScheme.East(coarseId))[0];
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        GeoHeightTile coarseTile = GeoHeightTileBuilder.Build(source, coarseId, 16);
        GeoHeightTile fineTile = GeoHeightTileBuilder.Build(source, fineId, 16);
        RenderMesh coarse = GeoTerrainMeshBuilder.Build(
            coarseTile, anchor, source, coverage)!;
        RenderMesh fine = GeoTerrainMeshBuilder.Build(fineTile,
            anchor, source, coverage,
            new GeoTerrainEdgeNeighbors(null, null, coarseId, null))!;
        RenderMesh unstitched = GeoTerrainMeshBuilder.Build(
            fineTile, anchor, source, coverage)!;

        GeoTileBounds cb = GeographicTileScheme.Bounds(coarseId);
        GeoTileBounds fb = GeographicTileScheme.Bounds(fineId);
        Vector3[] fineBorder = Enumerable.Range(0, 17)
            .Select(row => FineWest(fine, row)).ToArray();
        Vector3[] coarseBorder = Enumerable.Range(0, 17)
            .Select(row => CoarseEast(coarse, row)).ToArray();
        for (int row = 0; row <= 16; row++)
        {
            Vector3 actual = fineBorder[row];
            float distance = Enumerable.Range(0, 16).Min(i =>
                DistanceToSegment(actual, coarseBorder[i], coarseBorder[i + 1]));
            Assert.InRange(distance, 0f, 0.1f);
        }
        for (int row = 1; row < 16; row++)
        {
            double lat = cb.NorthLatitudeDegrees -
                (cb.NorthLatitudeDegrees - coverage.SouthLatitudeDegrees) * row / 16d;
            if (lat <= fb.SouthLatitudeDegrees || lat >= fb.NorthLatitudeDegrees)
                continue;
            Assert.Contains(fineBorder, point =>
                Vector3.Distance(point, coarseBorder[row]) < 0.1f);
        }
        Assert.Contains(Enumerable.Range(0, 17), i =>
            Vector3.Distance(fineBorder[i], FineWest(unstitched, i)) > 0.1f);;
        Assert.Equal(unstitched.TriangleCount, fine.TriangleCount);
    }

    private static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 edge = b - a;
        float t = Math.Clamp(Vector3.Dot(point - a, edge) /
            Vector3.Dot(edge, edge), 0f, 1f);
        return Vector3.Distance(point, a + t * edge);
    }

    private static Vector3 CoarseEast(RenderMesh mesh, int row)
    {
        int vertex = row == 16 ? 6 * (15 * 16 + 15) + 5 :
            6 * (row * 16 + 15) + 2;
        return Position(mesh, vertex);
    }

    private static Vector3 FineWest(RenderMesh mesh, int row)
    {
        int vertex = row == 16 ? 6 * (15 * 16) + 1 : 6 * (row * 16);
        return Position(mesh, vertex);
    }

    private static Vector3 Position(RenderMesh mesh, int vertex)
    {
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        int index = vertex * 7;
        return new Vector3(data[index], data[index + 1], data[index + 2]);
    }
}