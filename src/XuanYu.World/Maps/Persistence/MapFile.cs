
using System;
using System.IO;
using System.Text.Json;

namespace XuanYu.World.Maps;

public static class MapFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void Save(MapDocument map, string path)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var data = MapFileData.Capture(map);
        string json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(path, json);
    }

    public static MapDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string json = File.ReadAllText(path);
        MapFileData data = JsonSerializer.Deserialize<MapFileData>(json)
            ?? throw new InvalidDataException("地图文件缺少文档根对象。");
        data.Validate();
        return data.Restore();
    }
}
