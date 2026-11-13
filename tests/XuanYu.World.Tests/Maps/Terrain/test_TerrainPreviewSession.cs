
using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainPreviewSession
{
    [Fact]
    public void LargeMap_PickingReturnsUnscaledMeters()
    {
        var field = new TerrainHeightField(2, 2, 2000f);
        var map = new MapDocument("比例测试", field);
        var preview = new TerrainPreviewSession(map);

        Assert.InRange(preview.DisplayScale, 0.1279f, 0.1281f);
        Assert.True(preview.TryPick(new Vector2(400, 300),
            800, 600, out var hit));
        Assert.InRange(hit.WorldPosition.X, 999f, 1001f);
        Assert.InRange(hit.WorldPosition.Y, 999f, 1001f);
        Assert.InRange(hit.WorldPosition.Z, -0.01f, 0.01f);
    }
}
