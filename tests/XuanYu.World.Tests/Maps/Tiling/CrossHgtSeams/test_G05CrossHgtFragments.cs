using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G05CrossHgtFragments
{
    [Fact]
    public void AdjacentHgtFragments_UseTheSameBoundaryPositionsAndColors()
    {
        using var west = new G02BHgtFixture("N24E120");
        using var east = new G02BHgtFixture("N24E121");
        for (int row = 2193; row <= 2194; row++)
        {
            west.SetHeight(3600, row, 100);
            east.SetHeight(0, row, 2000);
        }
        var source = new NasademElevationSource(west.Load(), east.Load());
        var anchor = new EnuFrame(new GeoCoordinate(24.5, 121, 0));
        var id = new GeoTileId(8, 428, 93);
        var tile = GeoHeightTileBuilder.Build(source, id, 16);
        RenderMesh a = GeoTerrainMeshBuilder.Build(tile, anchor, source,
            new GeoTileBounds(24, 120, 25, 121))!;
        RenderMesh b = GeoTerrainMeshBuilder.Build(tile, anchor, source,
            new GeoTileBounds(24, 121, 25, 122))!;
        for (int row = 0; row <= 16; row++)
        {
            int av = row == 16 ? 6 * 255 + 5 : 6 * (row * 16 + 15) + 2;
            int bv = row == 16 ? 6 * 240 + 1 : 6 * row * 16;
            Assert.Equal(Vertex(a, av), Vertex(b, bv));
        }
    }

    private static float[] Vertex(RenderMesh mesh, int i) =>
        mesh.Vertices.Span.Slice(i * 7, 7).ToArray();
}