using System.Buffers.Binary;

namespace XuanYu.World.Maps;

public static partial class XYTerrainReader
{
    public static NasademTile ReadTile(string path)
    {
        using var stream = File.OpenRead(path);
        XYTerrainManifest manifest = XYTerrainFormat.Read(stream);
        var samples = new short[NasademTile.SampleCount];
        short min = short.MaxValue, max = short.MinValue;
        int noData = 0;
        for (int band = 0; band < manifest.Blocks.Length; band++)
        {
            byte[] raw = ReadBand(stream, manifest.Blocks[band], band);
            int start = band * XYTerrainFormat.BandRows * NasademTile.Side;
            for (int i = 0; i < raw.Length / 2; i++)
            {
                short value = BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(i * 2, 2));
                samples[start + i] = value;
                if (value == short.MinValue) { noData++; continue; }
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }
        if (noData == samples.Length)
            throw new InvalidDataException("XYTerrain 不包含有效高程数据。");
        NasademHgtInfo info = manifest.Info;
        return new NasademTile(info.TileId, info.SouthLatitude,
            info.WestLongitude, samples, noData, min, max);
    }
}