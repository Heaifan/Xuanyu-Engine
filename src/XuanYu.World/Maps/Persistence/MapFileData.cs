
using System;
using System.IO;

namespace XuanYu.World.Maps;

internal sealed class MapFileData
{
    public int Version { get; set; }
    public string Name { get; set; } = "";
    public int SamplesX { get; set; }
    public int SamplesY { get; set; }
    public float SpacingMeters { get; set; }
    public float[] Heights { get; set; } = [];

    public static MapFileData Capture(MapDocument map)
    {
        TerrainHeightField field = map.HeightField;
        float[] heights = new float[checked(field.SamplesX * field.SamplesY)];
        for (int y = 0; y < field.SamplesY; y++)
            for (int x = 0; x < field.SamplesX; x++)
                heights[y * field.SamplesX + x] = field.GetHeight(x, y);

        return new MapFileData
        {
            Version = MapDocument.CurrentVersion,
            Name = map.Name,
            SamplesX = field.SamplesX,
            SamplesY = field.SamplesY,
            SpacingMeters = field.SpacingMeters,
            Heights = heights
        };
    }

    public void Validate()
    {
        if (Version != MapDocument.CurrentVersion)
            throw new InvalidDataException($"不支持的地图文件版本：{Version}。");
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidDataException("地图名称不能为空。");
        if (SamplesX < 2 || SamplesY < 2 ||
            !float.IsFinite(SpacingMeters) || SpacingMeters <= 0f)
            throw new InvalidDataException("地图采样尺寸或间距无效。");
        if (Heights is null || Heights.LongLength != (long)SamplesX * SamplesY)
            throw new InvalidDataException("地图高度数据数量与采样尺寸不一致。");
        foreach (float height in Heights)
            if (!float.IsFinite(height))
                throw new InvalidDataException("地图高度数据包含非有限数值。");
    }

    public MapDocument Restore()
    {
        TerrainHeightField field = new(SamplesX, SamplesY, SpacingMeters);
        for (int y = 0; y < SamplesY; y++)
            for (int x = 0; x < SamplesX; x++)
                field.SetHeight(x, y, Heights[y * SamplesX + x]);
        return new MapDocument(Name, field);
    }
}
