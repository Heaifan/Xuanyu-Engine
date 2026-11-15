using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainLodIdle
{
    [Fact]
    public void MovingCamera_DoesNotReplaceChunksDuringGesture()
    {
        var map = new MapDocument("LOD 稳定性", new TerrainHeightField(601, 601, 2f));
        var preview = new TerrainPreviewSession(map);
        var before = preview.Scene.Objects.Select(item => item.Mesh).ToArray();
        preview.Scene.Camera!.Orbit(30f, 10f);
        preview.ViewportDraws(800, 600);
        var after = preview.Scene.Objects.Select(item => item.Mesh).ToArray();
        Assert.Equal(before.Length, after.Length);
        for (int i = 0; i < before.Length; i++)
            Assert.Same(before[i], after[i]);
    }
}