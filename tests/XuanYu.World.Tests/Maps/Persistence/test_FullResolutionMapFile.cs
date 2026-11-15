using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestFullResolutionMapFile
{
    [Fact]
    public void LargeMap_RoundTripsOriginalHeightsAndSpacing()
    {
        var field = new TerrainHeightField(1001, 1001, 28.2f, 30.8f);
        field.SetHeight(123, 456, 3210.5f);
        field.SetHeight(1000, 1000, -43f);
        var map = new MapDocument("全精度地图", field);
        string path = Path.Combine(AppContext.BaseDirectory,
            $"XYE-E407-{Guid.NewGuid():N}.xymap");
        try
        {
            MapFile.Save(map, path);
            var loaded = MapFile.Load(path);
            Assert.Equal(field.SampleCount, loaded.HeightField.SampleCount);
            Assert.Equal(3210.5f, loaded.HeightField.GetHeight(123, 456));
            Assert.Equal(-43f, loaded.HeightField.GetHeight(1000, 1000));
            Assert.Equal(28.2f, loaded.HeightField.SpacingXMeters);
            Assert.Equal(30.8f, loaded.HeightField.SpacingYMeters);
        }
        finally { File.Delete(path); }
    }
}