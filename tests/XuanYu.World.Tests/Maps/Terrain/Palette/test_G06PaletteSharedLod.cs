using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06PaletteSharedLod
{
    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public void SharedNorthWestSample_KeepsAbsoluteColorAcrossLods(int level)
    {
        using var fixture = new G02BHgtFixture("N24E121");
        GeoTileId coarseId = GeographicTileScheme.Find(
            new GeoCoordinate(24.5, 121.5, 0), level);
        GeoTileId fineId = GeographicTileScheme.Children(coarseId)[0];
        GeoTileBounds bounds = GeographicTileScheme.Bounds(coarseId);
        int x = (int)Math.Floor((bounds.WestLongitudeDegrees - 121d) * 3600d);
        int y = (int)Math.Floor((bounds.NorthLatitudeDegrees - 24d) * 3600d);
        for (int row = y; row <= y + 1; row++)
            for (int col = x; col <= x + 1; col++)
                fixture.SetHeight(col, row, 800);

        var source = new NasademElevationSource(fixture.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121.5, 0));
        var coverage = new GeoTileBounds(24, 121, 25, 122);
        RenderMesh coarse = Build(coarseId);
        RenderMesh fine = Build(fineId);
        Vector3 expected = new(0xA8 / 255f, 0xD9 / 255f, 0x6D / 255f);
        Assert.InRange(Vector3.Distance(Color(coarse), expected), 0f, 0.00002f);
        Assert.InRange(Vector3.Distance(Color(fine), expected), 0f, 0.00002f);
        Assert.Equal(coarse.Vertices.Span[..7].ToArray(), fine.Vertices.Span[..7].ToArray());

        RenderMesh Build(GeoTileId id) => GeoTerrainMeshBuilder.Build(
            GeoHeightTileBuilder.Build(source, id, 16),
            anchor, source, coverage)!;
    }

    private static Vector3 Color(RenderMesh mesh)
    {
        ReadOnlySpan<float> v = mesh.Vertices.Span;
        return new(v[3], v[4], v[5]);
    }
}