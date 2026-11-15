using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainWaterSave
{
    [Fact]
    public void WaterMask_RoundTripsAlongsideElevation()
    {
        var field = new TerrainHeightField(3, 2, 2.5f, 5.5f);
        field.SetHeight(2, 1, 135f);
        var water = new TerrainWaterMask(3, 2, [0, 0, 255, 255, 0, 0]);
        string path = Path.Combine(AppContext.BaseDirectory,
            $"XYE-WATER-{Guid.NewGuid():N}.xymap");
        try
        {
            MapFile.Save(new MapDocument("SWB 测试", field, water), path);
            var map = MapFile.Load(path);
            Assert.NotNull(map.WaterMask);
            Assert.True(map.WaterMask.IsWater(2, 0));
            Assert.True(map.WaterMask.IsWater(0, 1));
            Assert.False(map.WaterMask.IsWater(1, 0));
            Assert.Equal(135f, map.HeightField.GetHeight(2, 1));
            Assert.Equal(5.5f, map.HeightField.SpacingYMeters);
        }
        finally { File.Delete(path); }
    }
}