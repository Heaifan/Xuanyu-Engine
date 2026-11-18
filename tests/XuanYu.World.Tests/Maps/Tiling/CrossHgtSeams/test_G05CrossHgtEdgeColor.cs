using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05CrossHgtEdgeColor
{
    [Fact]
    public void FineEdge_UsesCoarsePositionAndColorAcrossTwoHgt()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var coarseId = new GeoTileId(8, 428, 93);
        var fineId = GeographicTileScheme.Children(GeographicTileScheme.North(coarseId)!.Value)[2];
        var b = GeographicTileScheme.Bounds(fineId);
        double lon = b.WestLongitudeDegrees + (b.EastLongitudeDegrees - b.WestLongitudeDegrees) * 3d / 16d;
        Spike(east, b.SouthLatitudeDegrees, lon);
        var source = new NasademElevationSource(west.Load(), east.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121, 0));
        var coverage = new GeoTileBounds(24, 120, 25, 122);
        Assert.Equal(2400d, source.Sample(b.SouthLatitudeDegrees, lon)!.Value, 6);
        RenderMesh coarse = GeoTerrainMeshBuilder.Build(GeoHeightTileBuilder.Build(source,
            coarseId, 16), anchor, source, coverage)!;
        var fineTile = GeoHeightTileBuilder.Build(source, fineId, 16);
        RenderMesh raw = GeoTerrainMeshBuilder.Build(fineTile, anchor, source, coverage)!;
        RenderMesh fine = GeoTerrainMeshBuilder.Build(fineTile, anchor, source, coverage,
            new GeoTerrainEdgeNeighbors(null, coarseId, null, null))!;
        Assert.True(Color(raw, South(3)).X > Color(coarse, North(0)).X + 0.2f);
        for (int i = 0; i <= 16; i++)
            AssertOnNorthEdge(coarse, Position(fine, South(i)), Color(fine, South(i)));
    }

    [Fact]
    public void NativeEdge_InterpolatesCoarseColorAtOriginalHgtBoundary()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        var id = GeographicTileScheme.Find(new GeoCoordinate(24.5, 121, 0), 13);
        var b = GeographicTileScheme.Bounds(id);
        Spike(east, b.NorthLatitudeDegrees, 121d);
        var a = west.Load();
        var source = new NasademElevationSource(a, east.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121, 0));
        var coverage = new GeoTileBounds(24, 120, 25, 122);
        RenderMesh coarse = GeoTerrainMeshBuilder.Build(GeoHeightTileBuilder.Build(source,
            id, 16), anchor, source, coverage)!;
        RenderMesh native = GeoTerrainNativeMeshBuilder.Build(a, id, anchor, source, coverage, default)!;
        Assert.Equal(2400d, source.Sample(b.NorthLatitudeDegrees, 121d)!.Value, 6);
        double t = (121d - b.WestLongitudeDegrees) / (b.EastLongitudeDegrees - b.WestLongitudeDegrees) * 16d;
        int k = (int)t;
        Vector3 expected = Vector3.Lerp(Position(coarse, North(k)), Position(coarse, North(k + 1)), (float)(t - k));
        int[] matches = Enumerable.Range(0, native.VertexCount).Where(i =>
            Vector3.Distance(Position(native, i), expected) < 0.1f).ToArray();
        Assert.NotEmpty(matches);
        foreach (int i in matches) AssertOnNorthEdge(coarse, Position(native, i), Color(native, i));
    }

    private static void Spike(G02BHgtFixture hgt, double lat, double lon)
    {
        int x = (int)Math.Floor((lon - 121d) * 3600d);
        int y = (int)Math.Floor((lat - 24d) * 3600d);
        for (int row = y; row <= y + 1; row++)
            for (int col = x; col <= x + 1; col++) hgt.SetHeight(col, row, 2400);
    }

    private static void AssertOnNorthEdge(RenderMesh mesh, Vector3 p, Vector3 color)
    {
        float best = float.MaxValue;
        Vector3 expected = default;
        for (int i = 0; i < 16; i++)
        {
            Vector3 a = Position(mesh, North(i)), b = Position(mesh, North(i + 1));
            Vector3 ab = b - a;
            float t = Math.Clamp(Vector3.Dot(p - a, ab) / ab.LengthSquared(), 0f, 1f);
            float error = Vector3.Distance(p, Vector3.Lerp(a, b, t));
            if (error >= best) continue;
            best = error;
            expected = Vector3.Lerp(Color(mesh, North(i)), Color(mesh, North(i + 1)), t);
        }
        Assert.InRange(best, 0f, 0.1f);
        Assert.InRange(Vector3.Distance(color, expected), 0f, 1e-5f);
    }

    private static int North(int i) => i == 16 ? 6 * 15 + 2 : 6 * i;
    private static int South(int i) => i == 16 ? 6 * 255 + 5 : 6 * (240 + i) + 1;
    private static Vector3 Position(RenderMesh m, int i) => At(m, i * 7);
    private static Vector3 Color(RenderMesh m, int i) => At(m, i * 7 + 3);
    private static Vector3 At(RenderMesh m, int i) => new(m.Vertices.Span[i], m.Vertices.Span[i + 1], m.Vertices.Span[i + 2]);
}