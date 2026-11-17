using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G02FTransition
{
    [Fact]
    public void Zoom_UpdatesMeshesWithoutOverlappingAncestorTiles()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        for (int i = 0; i < 128; i++) session.Draws(1000, 700);
        var before = session.VisibleTileIds.ToArray();
        Assert.NotEmpty(before);
        var priorMatrix = session.Draws(1000, 700)[0].ModelToClip;

        session.Navigate(0, 0, 16, false, 700);
        bool changed = false;
        for (int i = 0; i < 160; i++)
        {
            var draws = session.Draws(1000, 700);
            Assert.NotEmpty(draws);
            GeoTileId[] ids = session.VisibleTileIds.ToArray();
            var selected = ids.ToHashSet();
            foreach (GeoTileId id in ids)
            {
                GeoTileId? parent = GeographicTileScheme.Parent(id);
                while (parent is GeoTileId ancestor)
                {
                    Assert.DoesNotContain(ancestor, selected);
                    parent = GeographicTileScheme.Parent(ancestor);
                }
            }
            if (!ids.OrderBy(id => id.ToString()).SequenceEqual(
                before.OrderBy(id => id.ToString()))) changed = true;
        }
        Assert.True(changed);
        Assert.NotEqual(priorMatrix, session.Draws(1000, 700)[0].ModelToClip);
    }

    [Fact]
    public void GlobalTileWithoutOriginalGridSamples_UsesClippedHgt()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var source = new NasademElevationSource(hgt.Load());
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source,
            new GeoTileId(0, 1, 0), 16);
        Assert.Equal(0, tile.ValidSampleCount);
        var anchor = new EnuFrame(new GeoCoordinate(23.5d, 120.5d, 0d));
        var coverage = new GeoTileBounds(23d, 120d, 24d, 121d);
        var mesh = GeoTerrainMeshBuilder.Build(tile, anchor, source, coverage);
        Assert.NotNull(mesh);
        Assert.Equal(16 * 16 * 2, mesh.TriangleCount);
    }

    [Fact]
    public void PartialHgtTile_ClipsToSourceBoundsBeforeTriangulation()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var source = new NasademElevationSource(hgt.Load());
        var id = new GeoTileId(8, 427, 93);
        GeoHeightTile tile = GeoHeightTileBuilder.Build(source, id, 16);
        var anchor = new EnuFrame(new GeoCoordinate(23.5d, 120.5d, 0d));
        var coverage = new GeoTileBounds(23d, 120d, 24d, 121d);

        var mesh = GeoTerrainMeshBuilder.Build(tile, anchor, source, coverage);
        Assert.NotNull(mesh);
        Assert.Equal(16 * 16 * 2, mesh.TriangleCount);
    }
}