using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06Hillshade
{
    [Fact]
    public void CoarseAndFine_SharedSampleKeepsPaletteAndShade()
    {
        using var fixture = new G02BHgtFixture("N24E121");
        GeoTileId coarseId = GeographicTileScheme.Find(new GeoCoordinate(24.5, 121.5, 0), 12);
        GeoTileId fineId = GeographicTileScheme.Children(coarseId)[0];
        GeoTileBounds b = GeographicTileScheme.Bounds(coarseId);
        int x = (int)Math.Floor((b.WestLongitudeDegrees - 121d) * 3600d);
        int y = (int)Math.Floor((b.NorthLatitudeDegrees - 24d) * 3600d);
        for (int row = y - 2; row <= y + 3; row++)
            for (int col = x - 2; col <= x + 3; col++)
                fixture.SetHeight(col, row, (short)(800 + 30 * (col - x)));
        var source = new NasademElevationSource(fixture.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        var coarse = Build(coarseId);
        var fine = Build(fineId);
        Assert.Equal(coarse.VertexCount * 3, coarse.TerrainShadedColors.Length);
        Assert.Equal(fine.VertexCount * 3, fine.TerrainShadedColors.Length);
        Assert.Equal(coarse.Vertices.Span[..7].ToArray(), fine.Vertices.Span[..7].ToArray());
        Assert.Equal(coarse.TerrainShadedColors.Span[..3].ToArray(), fine.TerrainShadedColors.Span[..3].ToArray());
        Assert.InRange(Ratio(coarse, 0), 0.95f, 0.98f);
        Assert.Equal(1f, coarse.Vertices.Span[6]);

        RenderMesh Build(GeoTileId id) => GeoTerrainMeshBuilder.Build(
            GeoHeightTileBuilder.Build(source, id, 16), anchor, source, coverage)!;
    }

    [Fact]
    public void NativeMesh_WestFacingSlopeBrighterThanEastFacingSlope()
    {
        using var fixture = new G02BHgtFixture("N24E121");
        GeoTileId id = GeographicTileScheme.Find(new GeoCoordinate(24.5, 121.5, 0), 13);
        var b = GeographicTileScheme.Bounds(id);
        int x = (int)Math.Round(((b.WestLongitudeDegrees + b.EastLongitudeDegrees) / 2d - 121d) * 3600d);
        int y = (int)Math.Round(((b.SouthLatitudeDegrees + b.NorthLatitudeDegrees) / 2d - 24d) * 3600d);
        for (int row = y - 1; row <= y + 1; row++)
            for (int dx = -1; dx <= 1; dx++)
            {
                fixture.SetHeight(x - 4 + dx, row, (short)(800 + 30 * dx));
                fixture.SetHeight(x + 4 + dx, row, (short)(800 - 30 * dx));
            }
        var tile = fixture.Load();
        var source = new NasademElevationSource(tile);
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        var mesh = GeoTerrainNativeMeshBuilder.Build(tile, id, anchor, source,
            new GeoTileBounds(24, 121, 25, 122), default)!;
        int west = FindVertex(mesh, anchor, x - 4, y);
        int east = FindVertex(mesh, anchor, x + 4, y);
        Assert.Equal(mesh.VertexCount * 3, mesh.TerrainShadedColors.Length);
        Assert.Equal(mesh.Vertices.Span.Slice(west * 7 + 3, 4).ToArray(),
            mesh.Vertices.Span.Slice(east * 7 + 3, 4).ToArray());
        Assert.InRange(Ratio(mesh, west), 0.95f, 0.98f);
        Assert.InRange(Ratio(mesh, east), 0.76f, 0.80f);
    }

    private static float Ratio(RenderMesh mesh, int vertex) =>
        mesh.TerrainShadedColors.Span[vertex * 3 + 1] / mesh.Vertices.Span[vertex * 7 + 4];

    private static int FindVertex(RenderMesh mesh, EnuFrame anchor, int x, int y)
    {
        var p = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(24d + y / 3600d, 121d + x / 3600d, 0)));
        Vector3 expected = new((float)p.East, (float)p.North, (float)(p.Up + 800d));
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        for (int i = 0; i < mesh.VertexCount; i++)
            if (Vector3.Distance(new(data[i * 7], data[i * 7 + 1], data[i * 7 + 2]), expected) < 0.1f)
                return i;
        throw new InvalidOperationException("未找到原始 HGT 测试顶点。");
    }
}