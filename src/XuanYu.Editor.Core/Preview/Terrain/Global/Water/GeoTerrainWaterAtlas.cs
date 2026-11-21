using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoTerrainWaterAtlas
{
    private const int MaxSide = 4096;
    private const int WordsPerTile = (NasademTile.SampleCount + 15) / 16;

    public static RenderWaterAtlas? Build(GeoTileBounds area,
        IReadOnlyList<NasademHgtInfo> sources,
        Func<NasademHgtInfo, TerrainWaterMask?> load, out string status)
    {
        if (sources.Count == 0)
        {
            status = "无 SWB，不猜测水域";
            return null;
        }
        int south = (int)area.SouthLatitudeDegrees;
        int west = (int)area.WestLongitudeDegrees;
        int columns = (int)(area.EastLongitudeDegrees - west);
        int rows = (int)(area.NorthLatitudeDegrees - south);
        int header = 2 + columns * rows;
        long words = header + (long)sources.Count * WordsPerTile;
        if (words > (long)MaxSide * MaxSide)
        {
            status = "超出当前区域 SWB 容量，待分区流式接入";
            return null;
        }
        int side = (int)Math.Ceiling(Math.Sqrt(words));
        var packed = new short[side * side];
        packed[0] = (short)columns;
        packed[1] = (short)rows;
        int loaded = 0;
        foreach (NasademHgtInfo info in sources)
        {
            if (load(info) is not { } mask) continue;
            if (mask.SamplesX != NasademTile.Side || mask.SamplesY != NasademTile.Side)
                throw new InvalidDataException("SWB 原始掩膜尺寸错误。");
            int index = (info.SouthLatitude - south) * columns + info.WestLongitude - west;
            packed[2 + index] = (short)(loaded + 1);
            Pack(mask.Samples, packed.AsSpan(header + loaded * WordsPerTile, WordsPerTile));
            loaded++;
        }
        status = loaded == 0 ? "未读取到 SWB，不猜测水域" :
            $"已加载 {loaded}/{columns * rows} 个区域格（原始 SWB）";
        return loaded == 0 ? null : new RenderWaterAtlas(south, west, side, packed, loaded);
    }

    private static void Pack(ReadOnlySpan<byte> samples, Span<short> words)
    {
        // South-first 原始顺序不变；每个有符号字只承载 16 个水陆位。
        for (int i = 0; i < samples.Length; i += 16)
        {
            int bits = 0;
            int n = Math.Min(16, samples.Length - i);
            for (int b = 0; b < n; b++)
                if (samples[i + b] == 255) bits |= 1 << b;
            words[i / 16] = unchecked((short)bits);
        }
    }
}