using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainSelectionOutline
{
    [Fact]
    public void ChunkedTerrain_SelectOneChunkMarksEntireTerrain()
    {
        var field = new TerrainHeightField(601, 601, 1f);
        var session = new TerrainPreviewSession(new MapDocument("测试地形", field));
        Assert.Equal(4, session.Scene.Objects.Count);

        session.Scene.Select(session.Scene.Objects[0].Id);
        var draws = session.ViewportDraws(800, 600);
        Assert.Equal(4, draws.Count);
        Assert.All(draws, draw => Assert.True(draw.Selected));

        session.Scene.Select(null);
        var cleared = session.ViewportDraws(800, 600);
        Assert.All(cleared, draw => Assert.False(draw.Selected));
    }
}