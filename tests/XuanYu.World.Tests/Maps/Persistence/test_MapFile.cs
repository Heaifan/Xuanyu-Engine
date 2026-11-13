
using System;
using System.IO;
using Xunit;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestMapFile
{
    [Fact]
    public void SaveLoad_RestoresEveryHeightAndMetadata()
    {
        var field = new TerrainHeightField(3, 2, 2.5f);
        field.SetHeight(1, 0, -2.75f);
        field.SetHeight(2, 1, 10.125f);
        var original = new MapDocument("玄域测试地图", field);
        string path = NewPath();

        try
        {
            MapFile.Save(original, path);
            MapDocument loaded = MapFile.Load(path);
            Assert.Equal(original.Version, loaded.Version);
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(field.SamplesX, loaded.HeightField.SamplesX);
            Assert.Equal(field.SamplesY, loaded.HeightField.SamplesY);
            Assert.Equal(field.SpacingMeters, loaded.HeightField.SpacingMeters);
            for (int y = 0; y < field.SamplesY; y++)
                for (int x = 0; x < field.SamplesX; x++)
                    Assert.Equal(field.GetHeight(x, y), loaded.HeightField.GetHeight(x, y));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SaveAgain_ReplacesPreviousHeightData()
    {
        var field = new TerrainHeightField(2, 2, 1f);
        var map = new MapDocument("重复保存", field);
        string path = NewPath();

        try
        {
            MapFile.Save(map, path);
            field.SetHeight(1, 1, 7.25f);
            MapFile.Save(map, path);
            Assert.Equal(7.25f, MapFile.Load(path).HeightField.GetHeight(1, 1));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("""{"Version":999,"Name":"A","SamplesX":2,"SamplesY":2,"SpacingMeters":1,"Heights":[0,0,0,0]}""")]
    [InlineData("""{"Version":1,"Name":"A","SamplesX":2,"SamplesY":2,"SpacingMeters":1,"Heights":[0]}""")]
    [InlineData("""{"Version":1,"Name":"A","SamplesX":2,"SamplesY":2,"SpacingMeters":0,"Heights":[0,0,0,0]}""")]
    public void Load_RejectsInvalidMap(string json)
    {
        string path = NewPath();
        try
        {
            File.WriteAllText(path, json);
            Assert.Throws<InvalidDataException>(() => MapFile.Load(path));
        }
        finally { File.Delete(path); }
    }

    private static string NewPath() => Path.Combine(
        AppContext.BaseDirectory, $"XYE-E405-{Guid.NewGuid():N}.xymap");
}
