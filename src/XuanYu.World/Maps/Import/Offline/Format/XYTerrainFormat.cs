namespace XuanYu.World.Maps;

internal readonly record struct XYTerrainBlock(long Offset, int Length, int Codec);
internal readonly record struct XYTerrainManifest(NasademHgtInfo Info, XYTerrainBlock[] Blocks);

internal static class XYTerrainFormat
{
    public const int BandRows = 120;
    public const int BlockCount = (NasademTile.Side + BandRows - 1) / BandRows;
    public const int HeaderBytes = 7 * sizeof(int);
    private const int Magic = 0x31545958; // XYT1, little-endian

    public static void Write(BinaryWriter writer, NasademTile tile, XYTerrainBlock[] blocks)
    {
        writer.Write(Magic);
        writer.Write(1);
        writer.Write(tile.SouthLatitude);
        writer.Write(tile.WestLongitude);
        writer.Write(NasademTile.Side);
        writer.Write(BandRows);
        writer.Write(BlockCount);
        foreach (var block in blocks)
        {
            writer.Write(block.Offset);
            writer.Write(block.Length);
            writer.Write(block.Codec);
        }
    }

    public static XYTerrainManifest Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);
        if (stream.Length < HeaderBytes + BlockCount * 16L ||
            reader.ReadInt32() != Magic || reader.ReadInt32() != 1)
            throw new InvalidDataException("XYTerrain 版本或文件头错误。");
        int south = reader.ReadInt32(), west = reader.ReadInt32();
        if (south is < -90 or > 89 || west is < -180 or > 179 ||
            reader.ReadInt32() != NasademTile.Side ||
            reader.ReadInt32() != BandRows || reader.ReadInt32() != BlockCount)
            throw new InvalidDataException("XYTerrain 坐标或数据布局错误。");
        var blocks = new XYTerrainBlock[BlockCount];
        long next = HeaderBytes + BlockCount * 16L;
        for (int i = 0; i < blocks.Length; i++)
        {
            long offset = reader.ReadInt64();
            int length = reader.ReadInt32(), codec = reader.ReadInt32();
            if (offset != next || length <= 0 || codec is < 0 or > 3 ||
                offset + length > stream.Length)
                throw new InvalidDataException("XYTerrain 压缩块索引错误。");
            blocks[i] = new XYTerrainBlock(offset, length, codec);
            next += length;
        }
        if (next != stream.Length) throw new InvalidDataException("XYTerrain 文件尾部不匹配。");
        string id = $"{(south >= 0 ? 'N' : 'S')}{Math.Abs(south):D2}" +
            $"{(west >= 0 ? 'E' : 'W')}{Math.Abs(west):D3}";
        return new XYTerrainManifest(new NasademHgtInfo(id, south, west), blocks);
    }
}