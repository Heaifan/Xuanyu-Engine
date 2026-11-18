using System.IO;
using System.Text.Json;

namespace XuanYu.World.Maps;

public static partial class MapFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void Save(MapDocument map, string path)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (map.HeightField.SampleCount > 1_000_000 || map.WaterMask is not null)
        {
            SaveBinary(map, path);
            return;
        }
        var data = MapFileData.Capture(map);
        File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
    }

    public static MapDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var input = File.OpenRead(path);
        Span<byte> signature = stackalloc byte[8];
        int read = input.Read(signature);
        if (read == 8 && signature.SequenceEqual("XYEMAP04"u8))
            return LoadBinary(input, true, true);
        if (read == 8 && signature.SequenceEqual("XYEMAP03"u8))
            return LoadBinary(input, true, false);
        if (read == 8 && signature.SequenceEqual("XYEMAP02"u8))
            return LoadBinary(input, false, false);
        input.Dispose();
        string json = File.ReadAllText(path);
        MapFileData data = JsonSerializer.Deserialize<MapFileData>(json)
            ?? throw new InvalidDataException("地图文件缺少文档根对象。");
        data.Validate();
        return data.Restore();
    }
}