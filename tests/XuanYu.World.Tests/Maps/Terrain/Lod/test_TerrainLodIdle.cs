using Xunit;
using XuanYu.Editor.Preview;
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

    [Fact]
    public async Task ZoomOutAndBack_ReusesCachedMesh()
    {
        var field = new TerrainHeightField(901, 901, 2f);
        var preview = new TerrainPreviewSession(new MapDocument("LOD 缓存", field));
        var initial = preview.Scene.Objects.ToDictionary(x => x.Id, x => x.Mesh!);
        var camera = preview.Scene.Camera!;
        camera.Zoom(5f);
        PreviewObject? changed = null;
        DateTime limit = DateTime.UtcNow.AddSeconds(8);
        while (DateTime.UtcNow < limit && changed is null)
        {
            preview.ViewportDraws(800, 600);
            changed = preview.Scene.Objects.FirstOrDefault(x =>
                x.Mesh!.VertexCount > initial[x.Id].VertexCount);
            if (changed is null) await Task.Delay(10);
        }
        PreviewObject selected = Assert.IsType<PreviewObject>(changed);
        var refined = selected.Mesh;

        camera.Zoom(-5f);
        preview.ViewportDraws(800, 600);
        Assert.Same(initial[selected.Id], selected.Mesh);

        camera.Zoom(5f);
        preview.ViewportDraws(800, 600);
        Assert.Same(refined, selected.Mesh);
    }
}